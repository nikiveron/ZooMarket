using Review.Application.DTOs;
using Review.Application.Features.Reviews;

namespace Review.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> GetReviewByIdAsync(Guid id);
    Task<IEnumerable<ReviewDto>> GetReviewsByProductIdAsync(Guid productId);
    Task<IEnumerable<ReviewDto>> GetReviewsByUserIdAsync(Guid userId);
    Task<ProductReviewsSummaryDto> GetProductReviewsSummaryAsync(Guid productId);
    Task<ReviewDto> CreateReviewAsync(CreateReviewCommand command);
    Task UpdateReviewAsync(UpdateReviewCommand command);
    Task DeleteReviewAsync(Guid id);
}
