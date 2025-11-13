using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.DTOs;
using Payment.Application.Features.Payments;
using Payment.Application.Interfaces;

namespace Payment.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPaymentService _paymentService;

    public PaymentsController(IMediator mediator, IPaymentService paymentService)
    {
        _mediator = mediator;
        _paymentService = paymentService;
    }

    [HttpGet("order/{orderId:guid}")]
    public async Task<ActionResult<PaymentDto>> GetPaymentByOrder(Guid orderId)
    {
        var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
        if (payment == null)
            return NotFound();

        return Ok(payment);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByUser(Guid userId)
    {
        var payments = await _paymentService.GetPaymentsByUserIdAsync(userId);
        return Ok(payments);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentDto>> GetPayment(Guid id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
            return NotFound();

        return Ok(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResultDto>> ProcessPayment(ProcessPaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<ActionResult<PaymentResultDto>> RefundPayment(Guid id, [FromBody] decimal? amount = null)
    {
        var command = new RefundPaymentCommand(id, amount);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}