using Review.Application.DTOs;
using MediatR;

namespace Review.Application.Features.Reviews;

public record CreateReviewCommand(
    Guid ProductId,
    Guid UserId,
    int Rating,
    string Title,
    string Comment) : IRequest<ReviewDto>;

public record UpdateReviewCommand(
    Guid Id,
    int Rating,
    string Title,
    string Comment) : IRequest<Unit>;
