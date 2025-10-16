using Order.Application.DTOs;
using MediatR;

namespace Order.Application.Features.Orders;

public record CreateOrderCommand(
    Guid UserId,
    string ShippingAddress,
    string BillingAddress,
    List<CreateOrderItem> OrderItems) : IRequest<OrderDto>;

public record CreateOrderItem(
    Guid ProductId,
    int Quantity);

public record UpdateOrderStatusCommand(
    Guid OrderId,
    string Status) : IRequest<Unit>;

public record CancelOrderCommand(
    Guid OrderId) : IRequest<Unit>;
