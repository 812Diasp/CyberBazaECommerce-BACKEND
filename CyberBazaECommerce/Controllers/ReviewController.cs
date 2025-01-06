using CyberBazaECommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CyberBazaECommerce.Controllers
{
	[ApiController]
	[Route("api/products/{productId}/reviews")]
	public class ReviewController : ControllerBase
	{
		private readonly IReviewService _reviewService;

		public ReviewController(IReviewService reviewService)
		{
			_reviewService = reviewService;
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> AddReview(string productId, [FromBody] CreateReviewDto reviewDto)
		{
			if (User.Identity is not { IsAuthenticated: true })
			{
				return Unauthorized("User is not authenticated.");
			}
			var review = new Review
			{
				ProductId = productId, // Присваиваем productId
				Title = reviewDto.Title,
				Pros = reviewDto.Pros,
				Cons = reviewDto.Cons,
				Comment = reviewDto.Comment,
				Rating = reviewDto.Rating,
				UserId = User.FindFirst("uid")?.Value,
			};

			if (string.IsNullOrEmpty(review.UserId))
			{
				return BadRequest("UserId not found in JWT.");
			}
			await _reviewService.AddReviewAsync(productId, review);
			return CreatedAtAction(nameof(GetReviews), new { productId = productId }, review);
		}

		[HttpGet]
		public async Task<IActionResult> GetReviews(string productId)
		{
			var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
			return Ok(reviews);
		}
	}
}
