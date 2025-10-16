using MediatR;
using Order.Application.DTOs;

namespace Order.Application.Features.Carts;

public record AddCartItemCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity) : IRequest<CartDto>;

public record UpdateCartItemCommand(
    Guid UserId,
    Guid ProductId,
    int Quantity) : IRequest<CartDto>;

public record RemoveCartItemCommand(
    Guid UserId,
    Guid ProductId) : IRequest<Unit>;
