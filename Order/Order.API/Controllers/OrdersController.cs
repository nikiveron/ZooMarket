using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.API.Services;
using Order.Application.DTOs;
using Order.Application.Features.Orders;
using Order.Application.Interfaces;
using System.Security.Claims;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly OrderOrchestratorService _orchestrator;

    public OrdersController(IOrderService orderService, OrderOrchestratorService orchestrator)
    {
        _orderService = orderService;
        _orchestrator = orchestrator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders([FromQuery] Guid? userId = null)
    {
        var requestedUserId = userId;
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        if (User.IsInRole("Admin") == false && requestedUserId != currentUserId)
            return Forbid();

        IEnumerable<OrderDto> orders;

        if (userId.HasValue)
            orders = await _orderService.GetOrdersByUserIdAsync(userId.Value);
        else
            orders = await _orderService.GetAllOrdersAsync();

        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        // Получаем заказ по id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        // Если не админ, проверяем принадлежит ли заказ пользователю
        if (!User.IsInRole("Admin") && order.UserId != currentUserId)
            return Forbid();

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderCommand command)
    {
        var requestedUserId = command.UserId;
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        if (User.IsInRole("Admin") == false && requestedUserId != currentUserId)
            return Forbid();

        var result = await _orchestrator.ProcessOrderAsync(command);
        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        var order = await _orderService.GetOrderByIdAsync(result.OrderId);
        return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, order);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] string status)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        if (User.IsInRole("Admin") == false)
            return Forbid();

        await _orderService.UpdateOrderStatusAsync(new UpdateOrderStatusCommand(id, status));
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        // Получаем заказ по id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        // Если не админ, проверяем принадлежит ли заказ пользователю
        if (!User.IsInRole("Admin") && order.UserId != currentUserId)
            return Forbid();

        await _orderService.CancelOrderAsync(new CancelOrderCommand(id));
        return NoContent();
    }

    [HttpGet("owner/{orderId:guid}")]
    public async Task<ActionResult<Guid>> GetOrderOwnerId(Guid orderId)
    {
        if (!User.IsInRole("Admin"))
            return Forbid();
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound();

        return Ok(order.UserId);
    }
}