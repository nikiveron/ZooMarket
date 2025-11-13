using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs;
using Order.Application.Features.Orders;
using Order.Application.Interfaces;
using Order.API.Services;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderCommand command)
    {
        var result = await _orchestrator.ProcessOrderAsync(command);
        if (!result.Success)
            return BadRequest(new { error = result.ErrorMessage });

        var order = await _orderService.GetOrderByIdAsync(result.OrderId);
        return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId }, order);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] string status)
    {
        await _orderService.UpdateOrderStatusAsync(new UpdateOrderStatusCommand(id, status));
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        await _orderService.CancelOrderAsync(new CancelOrderCommand(id));
        return NoContent();
    }
}