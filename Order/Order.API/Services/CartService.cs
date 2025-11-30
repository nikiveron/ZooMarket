using Microsoft.EntityFrameworkCore;
using Order.Application.DTOs;
using Order.Application.Features.Carts;
using Order.Application.Interfaces;
using Order.Domain.Entities;
using Order.Infrastructure.Data;

namespace Order.API.Services;

public class CartService : ICartService
{
    private readonly OrderingDbContext _db;

    public CartService(OrderingDbContext db)
    {
        _db = db;
    }

    public async Task<CartDto> GetCartByUserIdAsync(Guid userId)
    {
        var cart = await _db.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return new CartDto
            {
                UserId = userId,
                CartItems = new List<CartItemDto>()
            };
        }

        return MapToDto(cart);
    }

    public async Task<CartDto> AddItemToCartAsync(AddCartItemCommand command)
    {
        var cart = await _db.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == command.UserId);

        // если корзины нет → создать
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = command.UserId,
                CartItems = new List<CartItem>()
            };
            _db.Carts.Add(cart);
        }

        var existingItem = cart.CartItems
            .FirstOrDefault(i => i.ProductId == command.ProductId);

        if (existingItem == null)
        {
            cart.CartItems.Add(new CartItem
            {
                ProductId = command.ProductId,
                Quantity = command.Quantity
            });
        }
        else
        {
            existingItem.Quantity += command.Quantity;
        }

        await _db.SaveChangesAsync();

        return MapToDto(cart);
    }

    public async Task<CartDto> UpdateCartItemAsync(UpdateCartItemCommand command)
    {
        var cart = await _db.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == command.UserId)
            ?? throw new KeyNotFoundException("Cart not found");

        var item = cart.CartItems.FirstOrDefault(i => i.ProductId == command.ProductId)
            ?? throw new KeyNotFoundException("Cart item not found");

        if (command.Quantity <= 0)
            throw new ArgumentException("Quantity must be > 0");

        item.Quantity = command.Quantity;

        await _db.SaveChangesAsync();

        return MapToDto(cart);
    }

    public async Task RemoveItemFromCartAsync(RemoveCartItemCommand command)
    {
        var cart = await _db.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == command.UserId)
            ?? throw new KeyNotFoundException("Cart not found");

        var item = cart.CartItems.FirstOrDefault(i => i.ProductId == command.ProductId)
            ?? throw new KeyNotFoundException("Cart item not found");

        _db.CartItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    public async Task ClearCartAsync(Guid userId)
    {
        var cart = await _db.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null) return;

        _db.CartItems.RemoveRange(cart.CartItems);
        await _db.SaveChangesAsync();
    }

    private CartDto MapToDto(Cart cart)
    {
        return new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            CartItems = cart.CartItems.Select(ci => new CartItemDto
            {
                ProductId = ci.ProductId,
                ProductName = $"Product {ci.ProductId}", // так же, как в OrderService — заглушка
                UnitPrice = 10.0m,                       // заглушка — каталога нет
                Quantity = ci.Quantity
            }).ToList()
        };
    }
}
