using CyberBazaECommerce.Models;
using CyberBazaECommerce.Services;
using CyberBazaECommerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using System.Text.Json;

namespace CyberBazaECommerce.Controllers
{

	[ApiController]
	[Route("api/products")]
	[EnableCors("AllowAll")]
	public class ProductsController : ControllerBase
	{
		private readonly ProductService _productService;
		private readonly CsrfValidator _csrfValidator;
		private readonly IDistributedCache _distributedCache;
		private readonly UserService _userService;

		public ProductsController(ProductService productService, CsrfValidator csrfValidator, IDistributedCache distributedCache,UserService userService)
		{
			_productService = productService;
			_csrfValidator = csrfValidator;
			_distributedCache = distributedCache;
			_userService = userService;
		}
		[HttpGet]
		public async Task<ActionResult<List<Product>>> GetProducts()
		{
			string recordKey = "productsList"; // Ключ для кэширования списка продуктов

			var cachedProducts = await _distributedCache.GetStringAsync(recordKey); // Проверяем, есть ли список в кэше

			if (cachedProducts != null) // Если данные есть в кеше
			{
				var products = JsonSerializer.Deserialize<List<Product>>(cachedProducts);
				var filteredProducts = products.Where(p => p.Category != "18+").ToList(); // Фильтруем данные из кэша

				return Ok(filteredProducts); // Возвращаем отфильтрованные данные из кэша
			}

			var productsFromDb = await _productService.GetProductsAsync(); // Получаем данные из сервиса (базы данных)

			var filteredProductsFromDb = productsFromDb.Where(p => p.Category != "18+").ToList(); // Фильтруем данные из базы данных

			var cacheOptions = new DistributedCacheEntryOptions()
				.SetAbsoluteExpiration(DateTime.Now.AddMinutes(5)); // Устанавливаем время жизни кэша (например, 5 минут)

			// Сериализуем данные в JSON
			await _distributedCache.SetStringAsync(recordKey, JsonSerializer.Serialize(filteredProductsFromDb), cacheOptions); // Сохраняем отфильтрованные данные в кэше

			return Ok(filteredProductsFromDb); // Возвращаем отфильтрованные данные из базы данных
		}
		[HttpPost("{productId}/favorites")]
		[Authorize]
		public async Task<IActionResult> AddToFavorites(string productId)
		{
			var userId = User.FindFirst("uid")?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("User ID not found in JWT");
			}

			var product = await _productService.GetProductByIdAsync(productId);
			if (product == null)
			{
				return NotFound("Product not found");
			}


			var user = await _userService.GetUserByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User not found");
			}


			if (user.Favorites.Contains(productId))
			{
				user.Favorites.Remove(productId);
				await _userService.UpdateUserAsync(user);
				return Ok("Product removed from favorites");
			}
			user.Favorites.Add(productId);
			var updateResult = await _userService.UpdateUserAsync(user);

			if (!updateResult)
			{
				return BadRequest("Failed to update user's favorites list");
			}

			return Ok("Product added to favorites");
		}

		[HttpPost("{productId}/tracking")]
		[Authorize]
		public async Task<IActionResult> AddToTracking(string productId)
		{
			var userId = User.FindFirst("uid")?.Value;

			if (string.IsNullOrEmpty(userId))
			{
				return BadRequest("User ID not found in JWT");
			}
			var product = await _productService.GetProductByIdAsync(productId);

			if (product == null)
			{
				return NotFound("Product not found");
			}

			var user = await _userService.GetUserByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User not found");
			}


			if (user.Tracking.Contains(productId))
			{
				user.Tracking.Remove(productId);
				await _userService.UpdateUserAsync(user);
				return Ok("Product removed from tracking");
			}

			user.Tracking.Add(productId);
			var updateResult = await _userService.UpdateUserAsync(user);

			if (!updateResult)
			{
				return BadRequest("Failed to update user's tracking list");
			}
			return Ok("Product added to tracking");
		}


		[HttpGet("{id}")]
		public async Task<ActionResult<Product>> GetProductById(string id)
		{
			var product = await _productService.GetProductByIdAsync(id);
			if (product == null)
			{
				return NotFound();
			}
			return Ok(product);
		}

		[HttpGet("discounted")]
		public async Task<ActionResult<List<Product>>> GetDiscountedProducts()
		{
			var discountedProducts = await _productService.GetProductsByDiscountAsync();
			return Ok(discountedProducts);
		}

		[HttpGet("category/{category}")]
		public async Task<ActionResult<List<Product>>> GetProductsByCategory(string category)
		{
			var products = await _productService.GetProductsByCategoryAsync(category);
			return Ok(products);
		}
		[HttpGet("search")]
		public async Task<ActionResult<List<Product>>> SearchProducts(string query)
		{
			if (string.IsNullOrWhiteSpace(query))
			{
				return Ok(new List<Product>()); 
			}

			var products = await _productService.SearchProductsAsync(query);
			return Ok(products);
		}

		[HttpGet("toprated")]
		public async Task<ActionResult<List<Product>>> GetTopRatedProducts()
		{
			var products = await _productService.GetTopRatedProductsAsync();
			return Ok(products);
		}
		
		[HttpGet("related/{productId}")]
		public async Task<IActionResult> GetRelatedProducts(string productId)
		{
			try
			{
				var product = await _productService.GetProductByIdAsync(productId);
				if (product == null) return NotFound();

				// Получаем все теги продукта
				var tags = product.Tags;

				//Получаем похожие продукты
				List<Product> relatedProducts = new List<Product>();
				foreach (var tag in tags)
				{
					var productsByTag = await _productService.GetProductsByTag(tag);
					relatedProducts.AddRange(productsByTag.Where(p => p.Id != productId));
				}

				//Возвращаем список
				return Ok(relatedProducts.DistinctBy(p => p.Id).Take(10)); // Take(10) для ограничения количества
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}



		[HttpPost]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto)
		{
			try
			{
				var product = new Product
				{
					Brand = productDto.Brand,
					Category = productDto.Category,
					MainImage = productDto.MainImage,
					AdditionalImages = productDto.AdditionalImages,
					Name = productDto.Name,
					Description = productDto.Description,
					Characteristics = productDto.Characteristics,
					Price = productDto.Price,
					OriginalPrice = productDto.OriginalPrice ?? productDto.Price,
					DiscountPercentage = productDto.DiscountPercentage,
					DiscountedPrice = productDto.DiscountedPrice,
					DiscountStartDate = productDto.DiscountStartDate,
					DiscountEndDate = productDto.DiscountEndDate,
					VatRate = productDto.VatRate,
					StockQuantity = productDto.StockQuantity,
					Weight = productDto.Weight,
					Dimensions = productDto.Dimensions,
					Tags = productDto.Tags,
					IsAvailable = productDto.IsAvailable,
					Manufacturer = productDto.Manufacturer,
					ShippingCost = productDto.ShippingCost,
					Vendor = productDto.Vendor,
					WarrantyPeriod = productDto.WarrantyPeriod,
					Colors = productDto.Colors,
					Options = productDto.Options
				};

				await _productService.CreateProductAsync(product);
				var createdProduct = await _productService.GetProductByIdAsync(product.Id);

				if (createdProduct == null)
				{
					return StatusCode(500, "Error occurred while creating the product.");
				}

				return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, createdProduct);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error occurred while creating the product: {ex.Message}");
			}
		}
		[HttpPost("many")]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> AddManyProducts([FromBody] CreateProductsDto productsDto)
		{
			// Validate CSRF token
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}

			// Validate input data
			if (productsDto == null || productsDto.Products == null || !productsDto.Products.Any())
			{
				return BadRequest("Need at least one product");
			}

			try
			{
				// Map DTOs to Product entities with updated fields
				var products = productsDto.Products.Select(dto => new Product
				{
					Brand = dto.Brand,
					Category = dto.Category,
					MainImage = dto.MainImage, // Updated field for main image
					AdditionalImages = dto.AdditionalImages, // Updated field for additional images
					Name = dto.Name,
					Description = dto.Description,
					Characteristics = dto.Characteristics,
					Price = dto.Price,
					OriginalPrice = dto.OriginalPrice ?? dto.Price,
					DiscountPercentage = dto.DiscountPercentage,
					DiscountedPrice = dto.DiscountedPrice,
					DiscountStartDate = dto.DiscountStartDate,
					DiscountEndDate = dto.DiscountEndDate,
					VatRate = dto.VatRate,
					StockQuantity = dto.StockQuantity,
					Weight = dto.Weight,
					Dimensions = dto.Dimensions,
					Tags = dto.Tags,
					IsAvailable = dto.IsAvailable,
					Manufacturer = dto.Manufacturer,
					ShippingCost = dto.ShippingCost,
					Vendor = dto.Vendor,
					WarrantyPeriod = dto.WarrantyPeriod,
					Colors = dto.Colors, // Updated field for colors
					Options = dto.Options // Updated field for options
				}).ToList();

				// Save products to the database
				await _productService.CreateManyProductsAsync(products);

				// Return success response
				return Ok("Products successfully added.");
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error occurred while adding products: {ex.Message}");
			}
		}
		[HttpPut("{id}")]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product updatedProduct)
		{
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}
			var existingProduct = await _productService.GetProductByIdAsync(id);
			if (existingProduct == null)
			{
				return NotFound();
			}
			await _productService.UpdateProductAsync(id, updatedProduct);
			return NoContent();
		}

		[HttpPatch("{id}/{fieldName}")]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> UpdateProductField(string id, string fieldName, [FromBody] object value)
		{
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}
			var existingProduct = await _productService.GetProductByIdAsync(id);
			if (existingProduct == null)
			{
				return NotFound();
			}
			try
			{
				await _productService.UpdateProductFieldAsync(id, fieldName, value);
				return NoContent();
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
		}
		[HttpDelete("{id}")]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> DeleteProduct(string id)
		{
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}
			var product = await _productService.GetProductByIdAsync(id);
			if (product == null)
			{
				return NotFound();
			}
			await _productService.DeleteProductAsync(id);
			return NoContent();
		}
	}
}
