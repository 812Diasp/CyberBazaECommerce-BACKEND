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

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto)
		{
			try
			{
				var product = new Product // смаппили из DTO в Product
				{
					Brand = productDto.Brand,
					Category = productDto.Category,
					Name = productDto.Name,
					Description = productDto.Description,
					Characteristics = productDto.Characteristics,
					Price = productDto.Price,
					DiscountedPrice = productDto.DiscountedPrice
				};

				await _productService.CreateProductAsync(product);
				// Важно! Получаем продукт из базы данных по Id после создания
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


		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product updatedProduct)
		{
			var existingProduct = await _productService.GetProductByIdAsync(id);
			if (existingProduct == null)
			{
				return NotFound();
			}

			await _productService.UpdateProductAsync(id, updatedProduct);
			return NoContent();
		}

		[HttpPatch("{id}/{fieldName}")]
		[Authorize]
		public async Task<IActionResult> UpdateProductField(string id, string fieldName, [FromBody] object value)
		{
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
		[Authorize]
		public async Task<IActionResult> DeleteProduct(string id)
		{
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
