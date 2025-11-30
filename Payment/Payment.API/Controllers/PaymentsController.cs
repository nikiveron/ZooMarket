using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payment.Domain.Entities;
using Payment.Infrastructure.Data;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentDbContext _context;
    private readonly IModel _rabbitMqChannel;

    public PaymentsController(PaymentDbContext context, IModel rabbitMqChannel)
    {
        _context = context;
        _rabbitMqChannel = rabbitMqChannel;
    }

    [HttpPost]
    public async Task<ActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
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
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
            return NotFound();

        return Ok(payment);
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult> GetPaymentByOrder(Guid orderId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.OrderId == orderId);
        
        if (payment == null)
            return NotFound();

        return Ok(payment);
    }
}

public class ProcessPaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public PaymentMethod Method { get; set; }
}
