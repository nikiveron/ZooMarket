using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs;
using Order.Application.Features.Carts;
using Order.Application.Interfaces;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICartService _cartService;

    public CartsController(IMediator mediator, ICartService cartService)
    {
        _mediator = mediator;
        _cartService = cartService;
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<CartDto>> GetCart(Guid userId)
    {
        var cart = await _cartService.GetCartByUserIdAsync(userId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItemToCart(AddCartItemCommand command)
    {
        var cart = await _mediator.Send(command);
        return Ok(cart);
    }

    [HttpPut("items")]
    public async Task<ActionResult<CartDto>> UpdateCartItem(UpdateCartItemCommand command)
    {
        var cart = await _mediator.Send(command);
        return Ok(cart);
    }

    [HttpDelete("items/{userId:guid}/{productId:guid}")]
    public async Task<IActionResult> RemoveItemFromCart(Guid userId, Guid productId)
    {
        var command = new RemoveCartItemCommand(userId, productId);
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("user/{userId:guid}")]
    public async Task<IActionResult> ClearCart(Guid userId)
    {
        await _cartService.ClearCartAsync(userId);
        return NoContent();
    }
}