using Microsoft.EntityFrameworkCore;
using Order.Application.DTOs;
using Order.Application.Features.Orders;
using Order.Application.Interfaces;
using Order.Domain.Common;
using Order.Domain.Entities;

namespace Order.API.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDto> GetOrderByIdAsync(Guid id)
    {
        var entity = await _orderRepository.GetByIdAsync(id);
        return entity == null ? null! : MapToDto(entity);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId)
    {
        var list = await _orderRepository.GetOrdersByUserIdAsync(userId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var userId = Guid.Empty; 
        var list = await _orderRepository.GetOrdersByUserIdAsync(userId);
        return list.Select(MapToDto).ToList();
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderCommand command)
    {
        var orderItemsData = command.OrderItems.Select(item =>
            new OrderItemData(item.ProductId, item.Quantity)).ToList();

        var entity = OrderEntity.Create(
            command.UserId,
            command.ShippingAddress,
            command.BillingAddress,
            orderItemsData); 

        await _orderRepository.AddAsync(entity);
        await _orderRepository.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task UpdateOrderStatusAsync(UpdateOrderStatusCommand command)
    {
        var entity = await _orderRepository.GetByIdAsync(command.OrderId) ?? throw new KeyNotFoundException("Order not found");
        if (Enum.TryParse<OrderStatus>(command.Status, true, out var newStatus))
        {
            entity.UpdateStatus(newStatus);
            await _orderRepository.UpdateAsync(entity);
            await _orderRepository.SaveChangesAsync();
        }
        else
        {
            throw new ArgumentException("Invalid status value", nameof(command.Status));
        }
    }

    public async Task CancelOrderAsync(CancelOrderCommand command)
    {
        var entity = await _orderRepository.GetByIdAsync(command.OrderId) ?? throw new KeyNotFoundException("Order not found");
        entity.Cancel();
        await _orderRepository.UpdateAsync(entity);
        await _orderRepository.SaveChangesAsync();
    }

    private static OrderDto MapToDto(OrderEntity entity)
    {
        return new OrderDto
        {
            Id = entity.Id,
            OrderNumber = entity.OrderNumber,
            UserId = entity.UserId,
            Status = entity.Status.ToString(),
            TotalAmount = entity.TotalAmount,
            ShippingAddress = entity.ShippingAddress,
            BillingAddress = entity.BillingAddress,
            CreatedAt = entity.CreatedAt,
            OrderItems = entity.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.ProductName,
                UnitPrice = oi.UnitPrice,
                Quantity = oi.Quantity
            }).ToList()
        };
    }
}


