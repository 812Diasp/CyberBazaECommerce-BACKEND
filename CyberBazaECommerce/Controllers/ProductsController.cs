using CyberBazaECommerce.Models;
using CyberBazaECommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CyberBazaECommerce.Controllers
{
	[ApiController]
	[Route("api/products")]
	public class ProductsController : ControllerBase
	{
		private readonly ProductService _productService;

		// Конструктор для внедрения зависимостей
		public ProductsController(ProductService productService)
		{
			_productService = productService;
		}
		[HttpGet]
		public async Task<ActionResult<List<Product>>> GetProducts()
		{
			var products = await _productService.GetProductsAsync();
			return Ok(products);
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
		private IActionResult ValidateCsrfToken()
		{
			string clientToken = Request.Headers["X-CSRF-Token"];
			if (string.IsNullOrEmpty(clientToken))
			{
				return BadRequest("X-CSRF-Token header is missing");
			}
			var serverToken = Request.Cookies["CSRF-TOKEN"];
			if (serverToken == null || serverToken != clientToken)
			{
				return StatusCode(403, "Invalid X-CSRF-Token");
			}
			return Ok();

		}


		[HttpPost]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto)
		{
			var validationResult = ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}
			try
			{
				var product = new Product
				{
					Brand = productDto.Brand,
					Category = productDto.Category,
					Name = productDto.Name,
					Description = productDto.Description,
					Image = productDto.Image,
					Characteristics = productDto.Characteristics,
					Price = productDto.Price,
					DiscountedPrice = productDto.DiscountedPrice
				};

				await _productService.CreateProductAsync(product);
				var createdProduct = await _productService.GetProductByIdAsync(product.Id);
				if (createdProduct == null)
				{
					return StatusCode(500, "Произошла ошибка при сохранении продукта. Попробуйте ещё раз.");
				}
				return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, createdProduct);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Ошибка при создании продукта: {ex.Message}");
			}
		}
		[HttpPost("many")] // Используем маршрут /api/products/many
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> AddManyProducts([FromBody] CreateProductsDto productsDto)
		{
			var validationResult = ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}

			if (productsDto == null || productsDto.Products == null || !productsDto.Products.Any())
			{
				return BadRequest("Необходимо предоставить хотя бы один продукт для добавления.");
			}

			try
			{
				var products = productsDto.Products.Select(dto => new Product
				{
					Brand = dto.Brand,
					Category = dto.Category,
					Name = dto.Name,
					Description = dto.Description,
					Image = dto.Image,
					Characteristics = dto.Characteristics,
					Price = dto.Price,
					DiscountedPrice = dto.DiscountedPrice
				}).ToList();

				await _productService.CreateManyProductsAsync(products);

				// Опционально: возвращаем созданные продукты (или их IDs)
				// Для этого, нужно будет доработать метод CreateManyProductsAsync, чтобы он возвращал список добавленных продуктов или их IDs

				return Ok("Продукты успешно добавлены."); // Или: return CreatedAtAction(nameof(GetProductById), products.Select(p => new { id = p.Id }), products)
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Ошибка при добавлении продуктов: {ex.Message}");
			}
		}

		[HttpPut("{id}")]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product updatedProduct)
		{
			var validationResult = ValidateCsrfToken();
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
			var validationResult = ValidateCsrfToken();
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
			var validationResult = ValidateCsrfToken();
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
