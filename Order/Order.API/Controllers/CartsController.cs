using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs;
using Order.Application.Features.Carts;
using Order.Application.Interfaces;
using System.Security.Claims;

namespace Order.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("user/{requestedUserId:guid}")]
    public async Task<ActionResult<CartDto>> GetCart(Guid requestedUserId)
    {
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid(); 

        if (User.IsInRole("Admin") == false && requestedUserId != currentUserId)
            return Forbid();

        var cart = await _cartService.GetCartByUserIdAsync(requestedUserId);
        return Ok(cart);
    }

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItemToCart(AddCartItemCommand command)
    {
        var requestedUserId = command.UserId;
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        if (User.IsInRole("Admin") == false && requestedUserId != currentUserId)
            return Forbid();

        var cart = await _cartService.AddItemToCartAsync(command);
        return Ok(cart);
    }

    [HttpPut("items")]
    public async Task<ActionResult<CartDto>> UpdateCartItem(UpdateCartItemCommand command)
    {
        var requestedUserId = command.UserId;
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        if (User.IsInRole("Admin") == false && requestedUserId != currentUserId)
            return Forbid();

        var cart = await _cartService.UpdateCartItemAsync(command);
        return Ok(cart);
    }

    [HttpDelete("items/{userId:guid}/{productId:guid}")]
    public async Task<IActionResult> RemoveItemFromCart(Guid userId, Guid productId)
    {
        var requestedUserId = userId;
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        if (User.IsInRole("Admin") == false && requestedUserId != currentUserId)
            return Forbid();

        var command = new RemoveCartItemCommand(userId, productId);
        await _cartService.RemoveItemFromCartAsync(command);
        return NoContent();
    }

    [HttpDelete("user/{userId:guid}")]
    public async Task<IActionResult> ClearCart(Guid userId)
    {
        var requestedUserId = userId;
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        if (User.IsInRole("Admin") == false && requestedUserId != currentUserId)
            return Forbid();

        await _cartService.ClearCartAsync(userId);
        return NoContent();
    }
}