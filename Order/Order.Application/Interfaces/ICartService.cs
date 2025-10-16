using Order.Application.DTOs;
using Order.Application.Features.Carts;

namespace Order.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartByUserIdAsync(Guid userId);
    Task<CartDto> AddItemToCartAsync(AddCartItemCommand command);
    Task<CartDto> UpdateCartItemAsync(UpdateCartItemCommand command);
    Task RemoveItemFromCartAsync(RemoveCartItemCommand command);
    Task ClearCartAsync(Guid userId);
}
