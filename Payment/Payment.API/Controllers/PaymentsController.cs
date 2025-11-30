using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;
using Payment.Infrastructure.Data;
using RabbitMQ.Client;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly PaymentDbContext _context;
    private readonly IModel _rabbitMqChannel;
    private readonly HttpClient _orderServiceClient;

    public PaymentsController(PaymentDbContext context, IModel rabbitMqChannel, HttpClient orderServiceClient)
    {
        _context = context;
        _rabbitMqChannel = rabbitMqChannel;
        _orderServiceClient = orderServiceClient;
    }

    [HttpPost]
    public async Task<ActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        if (User.IsInRole("Admin") == false)
            return Forbid();

        var payment = new PaymentEntity
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            Currency = request.Currency ?? "USD",
            Method = request.Method,
            Status = PaymentStatus.Processing
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Simulate payment processing
        await Task.Delay(100);
        payment.Status = PaymentStatus.Completed;
        payment.PaidAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Publish event to RabbitMQ
        var eventData = new
        {
            PaymentId = payment.Id,
            OrderId = payment.OrderId,
            Status = payment.Status.ToString(),
            Amount = payment.Amount
        };
        var message = JsonSerializer.Serialize(eventData);
        var body = Encoding.UTF8.GetBytes(message);
        _rabbitMqChannel.BasicPublish(exchange: "", routingKey: "payment_events", body: body);

        return Ok(new { PaymentId = payment.Id, Status = payment.Status });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetPayment(Guid id)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
            return NotFound();

        Guid? orderOwnerId = await GetOrderOwnerId(payment.OrderId); // вручную из Orders БД или через OrderService
        if (!User.IsInRole("Admin") && orderOwnerId != currentUserId && orderOwnerId != null)
            return Forbid();

        return Ok(payment);
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult> GetPaymentByOrder(Guid orderId)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId);

        if (payment == null)
            return NotFound();

        Guid? orderOwnerId = await GetOrderOwnerId(payment.OrderId); // вручную из Orders БД или через OrderService
        if (!User.IsInRole("Admin") && orderOwnerId != currentUserId && orderOwnerId != null)
            return Forbid();

        return Ok(payment);
    }

    private async Task<Guid?> GetOrderOwnerId(Guid orderId)
    {
        var response = await _orderServiceClient.GetAsync($"/api/orders/owner/{orderId}");
        if (!response.IsSuccessStatusCode)
            return null;

        var userIdString = await response.Content.ReadAsStringAsync();
        if (Guid.TryParse(userIdString.Trim('"'), out var userId))
            return userId;
        return null;
    }
}

public class ProcessPaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public PaymentMethod Method { get; set; }
}
