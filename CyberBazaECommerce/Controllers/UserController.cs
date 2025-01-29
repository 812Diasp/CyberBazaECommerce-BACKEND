using CyberBazaECommerce.Models;
using CyberBazaECommerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CyberBazaECommerce.Controllers
{
	[ApiController]
	[Route("api/users")]
	[Authorize]
	[EnableCors("AllowAll")] // Добавляем EnableCors
	public class UserController : ControllerBase
	{
		private readonly UserService _userService;
		private readonly CsrfValidator _csrfValidator;


		public UserController(UserService userService, CsrfValidator csrfValidator)
		{
			_userService = userService;
			_csrfValidator = csrfValidator;
		}


		[HttpGet("me")]
		[Authorize]
		public async Task<ActionResult<User>> GetMe()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}
			var user = await _userService.GetUserByIdAsync(userId);
			if (user == null)
			{
				return NotFound();
			}
			return Ok(user.Email);
		}
		[HttpGet("userCart")]
		public async Task<ActionResult<List<CartItem>>> GetCart()
		{
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}
			var user = await _userService.GetUserByIdAsync(userId);
			if (user == null)
			{
				return NotFound();
			}
			return Ok(user.Cart);

		}
		[HttpPost("cart/add/{productId}/{quantity}")]
		public async Task<IActionResult> AddItemToCart(string productId, int quantity)
		{
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}
			if (quantity <= 0)
			{
				return BadRequest("Quantity must be greater than zero.");
			}
			await _userService.AddItemToCartAsync(userId, productId, quantity);
			return Ok("item added");
		}

		[HttpDelete("cart/remove/{productId}")]
		public async Task<IActionResult> RemoveItemFromCart(string productId)
		{
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}
			await _userService.RemoveItemFromCartAsync(userId, productId);
			return Ok("Item removed");
		}

		[HttpPatch("cart/update/{productId}/{quantity}")]
		public async Task<IActionResult> UpdateCartItemQuantity(string productId, int quantity)
		{
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}
			if (quantity <= 0)
			{
				return BadRequest("Quantity must be greater than zero.");
			}
			await _userService.UpdateCartItemQuantityAsync(userId, productId, quantity);
			return Ok("Cart item quantity updated");
		}

		[HttpDelete("cart/clear")]
		public async Task<IActionResult> ClearCart()
		{
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}
			var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (userId == null)
			{
				return Unauthorized();
			}
			await _userService.ClearCartAsync(userId);
			return Ok("Cart cleared");
		}
	}
}
