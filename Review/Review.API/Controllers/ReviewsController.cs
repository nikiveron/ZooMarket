using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Review.Domain.Entities;
using Review.Infrastructure.Data;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Review.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly ReviewDbContext _context;
    private readonly IModel _rabbitMqChannel;

    public ReviewsController(ReviewDbContext context, IModel rabbitMqChannel)
    {
        _context = context;
        _rabbitMqChannel = rabbitMqChannel;
    }

    [HttpPost]
    public async Task<ActionResult> CreateReview([FromBody] CreateReviewRequest request)
    {
        var requestedUserId = request.UserId;
        var currentUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Проверяем что токен был валиден и claim реально существует
        if (string.IsNullOrWhiteSpace(currentUserIdString) || !Guid.TryParse(currentUserIdString, out var currentUserId))
            return Forbid();

        if (User.IsInRole("Admin") == false && requestedUserId != currentUserId)
            return Forbid();

        var review = new ReviewEntity
        {
            ProductId = request.ProductId,
            UserId = request.UserId,
            UserName = request.UserName,
            Rating = request.Rating,
            Title = request.Title,
            Comment = request.Comment,
            IsVerifiedPurchase = request.IsVerifiedPurchase,
            ReviewDate = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Update summary
        await UpdateProductSummary(review.ProductId);

        // Publish event to RabbitMQ
        var eventData = new
        {
            ReviewId = review.Id,
            ProductId = review.ProductId,
            Rating = review.Rating
        };
        var message = JsonSerializer.Serialize(eventData);
        var body = Encoding.UTF8.GetBytes(message);
        _rabbitMqChannel.BasicPublish(exchange: "", routingKey: "review_events", body: body);

        return Ok(review);
    }

    [HttpGet("product/{productId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetReviewsByProduct(Guid productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();
        return Ok(reviews);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetReview(Guid id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review == null)
            return NotFound();
        return Ok(review);
    }

    private async Task UpdateProductSummary(Guid productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        var summary = await _context.ProductReviewsSummaries
            .FirstOrDefaultAsync(s => s.ProductId == productId);

        if (summary == null)
        {
            summary = new ProductReviewsSummary { ProductId = productId };
            _context.ProductReviewsSummaries.Add(summary);
        }

        summary.TotalReviews = reviews.Count;
        summary.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
        summary.FiveStarCount = reviews.Count(r => r.Rating == 5);
        summary.FourStarCount = reviews.Count(r => r.Rating == 4);
        summary.ThreeStarCount = reviews.Count(r => r.Rating == 3);
        summary.TwoStarCount = reviews.Count(r => r.Rating == 2);
        summary.OneStarCount = reviews.Count(r => r.Rating == 1);

        await _context.SaveChangesAsync();
    }
}

public class CreateReviewRequest
{
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsVerifiedPurchase { get; set; }
}
