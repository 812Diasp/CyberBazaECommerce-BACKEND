using CyberBazaECommerce.Models;
using CyberBazaECommerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CyberBazaECommerce.Controllers
{
	[ApiController]
	[Route("api/products/{productId}/reviews")]
	[EnableCors("AllowAll")]
	public class ReviewController : ControllerBase
	{
		private readonly IReviewService _reviewService;
		private readonly CsrfValidator _csrfValidator;
		

		public ReviewController(IReviewService reviewService, CsrfValidator csrfValidator)
		{
			_reviewService = reviewService;
			_csrfValidator = csrfValidator;
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> AddReview(string productId, [FromBody] CreateReviewDto reviewDto)
		{
			if (User.Identity is not { IsAuthenticated: true })
			{
				return Unauthorized("User is not authenticated.");
			}

			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}
			var review = new Review
			{
				ProductId = productId,
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
			try
			{
				await _reviewService.AddReviewAsync(productId, review);
				return CreatedAtAction(nameof(GetReviews), new { productId = productId }, review);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error adding review: {ex.Message}");
			}

		}

		[HttpGet]
		public async Task<IActionResult> GetReviews(string productId)
		{
			var reviews = await _reviewService.GetReviewsByProductIdAsync(productId);
			return Ok(reviews);
		}
	}
}
