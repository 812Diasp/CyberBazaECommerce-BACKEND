using CyberBazaECommerce.Models;
using MongoDB.Driver;

namespace CyberBazaECommerce.Services
{
	public class ProductService
	{
		private readonly IMongoCollection<Product> _products;

		public ProductService(IMongoDatabase database)
		{
			_products = database.GetCollection<Product>("products");
		}

		public async Task<List<Product>> GetProductsAsync()
		{
			return await _products.Find(_ => true).ToListAsync();
		}

		public async Task<Product> GetProductByIdAsync(string id)
		{
			return await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
		}

		public async Task<List<Product>> GetProductsByDiscountAsync()
		{
			return await _products.Find(p => p.DiscountedPrice != null).ToListAsync();
		}

		public async Task<List<Product>> GetProductsByCategoryAsync(string category)
		{
			return await _products.Find(p => p.Category == category).ToListAsync();
		}

		public async Task CreateProductAsync(Product product)
		{
			try
			{
				if (!string.IsNullOrEmpty(product.Id))
				{
					throw new ArgumentException("Поле Id должно быть пустым для автогенерации.");
				}
				await _products.InsertOneAsync(product);
			}
			catch (MongoWriteException ex)
			{
				Console.WriteLine($"Ошибка при вставке продукта: {ex.Message}");
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при вставке продукта: {ex.Message}");
				throw;
			}
		}

		public async Task UpdateProductAsync(string id, Product updatedProduct)
		{
			try
			{
				await _products.ReplaceOneAsync(p => p.Id == id, updatedProduct);
			}
			catch (MongoWriteException ex)
			{
				Console.WriteLine($"Ошибка при обновлении продукта: {ex.Message}");
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при обновлении продукта: {ex.Message}");
				throw;
			}
		}

		public async Task UpdateProductFieldAsync(string id, string fieldName, object value)
		{
			try
			{
				var update = Builders<Product>.Update.Set(fieldName, value);
				await _products.UpdateOneAsync(p => p.Id == id, update);
			}
			catch (MongoWriteException ex)
			{
				Console.WriteLine($"Ошибка при обновлении поля продукта: {ex.Message}");
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при обновлении поля продукта: {ex.Message}");
				throw;
			}
		}

		public async Task DeleteProductAsync(string id)
		{
			try
			{
				await _products.DeleteOneAsync(p => p.Id == id);
			}
			catch (MongoWriteException ex)
			{
				Console.WriteLine($"Ошибка при удалении продукта: {ex.Message}");
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при удалении продукта: {ex.Message}");
				throw;
			}
		}
	}
}
