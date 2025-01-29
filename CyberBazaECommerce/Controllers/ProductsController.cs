using CyberBazaECommerce.Models;
using CyberBazaECommerce.Services;
using CyberBazaECommerce.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace CyberBazaECommerce.Controllers
{
	
	[ApiController]
	[Route("api/products")]
	[EnableCors("AllowAll")]
	public class ProductsController : ControllerBase
	{
		private readonly ProductService _productService;
		private readonly CsrfValidator _csrfValidator;


		public ProductsController(ProductService productService, CsrfValidator csrfValidator)
		{
			_productService = productService;
			_csrfValidator = csrfValidator;
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
		[HttpGet("search")]
		public async Task<ActionResult<List<Product>>> SearchProducts(string query)
		{
			var products = await _productService.SearchProductsAsync(query);
			return Ok(products);
		}

		[HttpGet("toprated")]
		public async Task<ActionResult<List<Product>>> GetTopRatedProducts()
		{
			var products = await _productService.GetTopRatedProductsAsync();
			return Ok(products);
		}
		[HttpGet("sku/{sku}")]
		public async Task<ActionResult<Product>> GetProductBySku(string sku)
		{
			var product = await _productService.GetProductBySkuAsync(sku);
			if (product == null)
			{
				return NotFound();
			}
			return Ok(product);
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
			var validationResult = _csrfValidator.ValidateCsrfToken();
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
					Image = productDto.Image,
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
					SKU = productDto.SKU,
					Tags = productDto.Tags,
					IsAvailable = productDto.IsAvailable,
					Manufacturer = productDto.Manufacturer,
					ShippingCost = productDto.ShippingCost,
					Vendor = productDto.Vendor,
					WarrantyPeriod = productDto.WarrantyPeriod
				};
				await _productService.CreateProductAsync(product);
				var createdProduct = await _productService.GetProductByIdAsync(product.Id);
				if (createdProduct == null)
				{
					return StatusCode(500, "Error oqured in creating product.");
				}
				return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, createdProduct);
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error oqured creating product: {ex.Message}");
			}
		}

		[HttpPost("many")]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> AddManyProducts([FromBody] CreateProductsDto productsDto)
		{
			var validationResult = _csrfValidator.ValidateCsrfToken();
			if (validationResult is not OkResult)
			{
				return validationResult;
			}
			if (productsDto == null || productsDto.Products == null || !productsDto.Products.Any())
			{
				return BadRequest("Need at least one product");
			}
			try
			{
				var products = productsDto.Products.Select(dto => new Product
				{
					Brand = dto.Brand,
					Category = dto.Category,
					Image = dto.Image,
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
					SKU = dto.SKU,
					Tags = dto.Tags,
					IsAvailable = dto.IsAvailable,
					Manufacturer = dto.Manufacturer,
					ShippingCost = dto.ShippingCost,
					Vendor = dto.Vendor,
					WarrantyPeriod = dto.WarrantyPeriod
				}).ToList();
				await _productService.CreateManyProductsAsync(products);
				return Ok("Product Successfully added."); // Или: return CreatedAtAction(nameof(GetProductById), products.Select(p => new { id = p.Id }), products)
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"error of adding products: {ex.Message}");
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
