using CyberBazaECommerce.Models;
using CyberBazaECommerce.Services;
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
		private readonly ProductService _productService;


		public UserController(UserService userService, ProductService productService, CsrfValidator csrfValidator)
		{
			_userService = userService;
			_csrfValidator = csrfValidator;
			_productService = productService;
		}


		[HttpGet("me")]
		[Authorize]
		public async Task<ActionResult<User>> GetMe()
		{
			var userId = User.FindFirst("uid")?.Value; // Используем наш "uid" claim

			if (userId == null)
			{
				return Unauthorized();
			}

			var user = await _userService.GetUserByIdAsync(userId);

			if (user == null)
			{
				return NotFound();
			}

			return Ok(user); // Возвращаем весь объект пользователя
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
		// GET: api/users/favorites
		[HttpGet("favorites")]
		public async Task<ActionResult<IEnumerable<Product>>> GetFavorites()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Получаем ID текущего пользователя
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User not authenticated.");
			}

			// Получаем пользователя из базы данных
			var user = await _userService.GetUserByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User not found.");
			}

			// Получаем список ID товаров из Favorites
			var favoriteIds = user.Favorites ?? new List<string>();

			// Получаем полные данные о товарах
			var favoriteProducts = await GetProductsByIds(favoriteIds);

			return Ok(favoriteProducts);
		}

		// GET: api/users/tracking
		[HttpGet("tracking")]
		public async Task<ActionResult<IEnumerable<Product>>> GetTracking()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Получаем ID текущего пользователя
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User not authenticated.");
			}

			// Получаем пользователя из базы данных
			var user = await _userService.GetUserByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User not found.");
			}

			// Получаем список ID товаров из Tracking
			var trackingIds = user.Tracking ?? new List<string>();

			// Получаем полные данные о товарах
			var trackingProducts = await GetProductsByIds(trackingIds);

			return Ok(trackingProducts);
		}

		// Вспомогательный метод для получения товаров по списку ID
		private async Task<List<Product>> GetProductsByIds(IEnumerable<string> productIds)
		{
			var products = new List<Product>();

			foreach (var productId in productIds)
			{
				var product = await _productService.GetProductByIdAsync(productId);
				if (product != null)
				{
					products.Add(product);
				}
			}

			return products;
		}
		[HttpPost("cart/add/{productId}/{quantity}")]
		public async Task<IActionResult> AddItemToCart(string productId, int quantity, string color, string? variant)
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

			// Вызов обновленного метода UserService
			await _userService.AddItemToCartAsync(userId, productId, quantity, color, variant);

			return Ok("Item added to cart");
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
		public async Task<IActionResult> UpdateCartItemQuantity(string productId, int quantity, string color, string? variant = null)
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

			await _userService.UpdateCartItemQuantityAsync(userId, productId, quantity, color, variant);
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
