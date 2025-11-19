using MediatR;
using Microsoft.AspNetCore.Mvc;
using Review.Application.DTOs;
using Review.Application.Features.Reviews;
using Review.Application.Interfaces;

namespace Review.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IReviewService _reviewService;

    public ReviewsController(IMediator mediator, IReviewService reviewService)
    {
        _mediator = mediator;
        _reviewService = reviewService;
    }

    [HttpGet("product/{productId:guid}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByProduct(Guid productId)
    {
        var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
        return Ok(reviews);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviewsByUser(Guid userId)
    {
        var reviews = await _reviewService.GetReviewsByUserIdAsync(userId);
        return Ok(reviews);
    }

    [HttpGet("product/{productId:guid}/summary")]
    public async Task<ActionResult<ProductReviewsSummaryDto>> GetProductReviewsSummary(Guid productId)
    {
        var summary = await _reviewService.GetProductReviewsSummaryAsync(productId);
        return Ok(summary);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReviewDto>> GetReview(Guid id)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        if (review == null)
            return NotFound();

        return Ok(review);
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> CreateReview(CreateReviewCommand command)
    {
        var review = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetReview), new { id = review.Id }, review);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateReview(Guid id, UpdateReviewCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteReview(Guid id)
    {
        await _reviewService.DeleteReviewAsync(id);
        return NoContent();
    }
}