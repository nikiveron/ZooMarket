using Order.Application.DTOs;
using Order.Application.Features.Orders;

namespace Order.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> GetOrderByIdAsync(Guid id);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<OrderDto> CreateOrderAsync(CreateOrderCommand command);
    Task UpdateOrderStatusAsync(UpdateOrderStatusCommand command);
    Task CancelOrderAsync(CancelOrderCommand command);
}
