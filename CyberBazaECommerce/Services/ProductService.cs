using CyberBazaECommerce.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CyberBazaECommerce.Services
{
	//public class ProductService
	//{
	//	private readonly IMongoCollection<Product> _products;

	//	public ProductService(IMongoDatabase database)
	//	{
	//		_products = database.GetCollection<Product>("Products");
	//	}

	//	public async Task<List<Product>> GetProductsAsync()
	//	{
	//		return await _products.Find(_ => true).ToListAsync();
	//	}

	//	public async Task<Product> GetProductByIdAsync(string id)
	//	{
	//		return await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
	//	}

	//	public async Task<List<Product>> GetProductsByDiscountAsync()
	//	{
	//		return await _products.Find(p => p.DiscountedPrice != null).ToListAsync();
	//	}

	//	public async Task<List<Product>> GetProductsByCategoryAsync(string category)
	//	{
	//		return await _products.Find(p => p.Category == category).ToListAsync();
	//	}

	//	public async Task CreateProductAsync(Product product)
	//	{
	//		try
	//		{
	//			if (!string.IsNullOrEmpty(product.Id))
	//			{
	//				throw new ArgumentException("Поле Id должно быть пустым для автогенерации.");
	//			}
	//			await _products.InsertOneAsync(product);
	//		}
	//		catch (MongoWriteException ex)
	//		{
	//			Console.WriteLine($"Ошибка при вставке продукта: {ex.Message}");
	//			throw;
	//		}
	//		catch (Exception ex)
	//		{
	//			Console.WriteLine($"Ошибка при вставке продукта: {ex.Message}");
	//			throw;
	//		}
	//	}
	//	public async Task CreateManyProductsAsync(List<Product> products)
	//	{
	//		try {
	//			await _products.InsertManyAsync(products);

	//		}
	//		catch (MongoWriteException ex)
	//		{
	//			Console.WriteLine($"Ошибка при вставке продуктов: {ex.Message}");
	//			throw;
	//		}
	//		catch (Exception ex)
	//		{
	//			Console.WriteLine($"Ошибка при вставке продуктов: {ex.Message}");
	//			throw;
	//		}
	//	}
	//	public async Task UpdateProductAsync(string id, Product updatedProduct)
	//	{
	//		try
	//		{
	//			await _products.ReplaceOneAsync(p => p.Id == id, updatedProduct);
	//		}
	//		catch (MongoWriteException ex)
	//		{
	//			Console.WriteLine($"Ошибка при обновлении продукта: {ex.Message}");
	//			throw;
	//		}
	//		catch (Exception ex)
	//		{
	//			Console.WriteLine($"Ошибка при обновлении продукта: {ex.Message}");
	//			throw;
	//		}
	//	}

	//	public async Task UpdateProductFieldAsync(string id, string fieldName, object value)
	//	{
	//		try
	//		{
	//			var update = Builders<Product>.Update.Set(fieldName, value);
	//			await _products.UpdateOneAsync(p => p.Id == id, update);
	//		}
	//		catch (MongoWriteException ex)
	//		{
	//			Console.WriteLine($"Ошибка при обновлении поля продукта: {ex.Message}");
	//			throw;
	//		}
	//		catch (Exception ex)
	//		{
	//			Console.WriteLine($"Ошибка при обновлении поля продукта: {ex.Message}");
	//			throw;
	//		}
	//	}

	//	public async Task DeleteProductAsync(string id)
	//	{
	//		try
	//		{
	//			await _products.DeleteOneAsync(p => p.Id == id);
	//		}
	//		catch (MongoWriteException ex)
	//		{
	//			Console.WriteLine($"Ошибка при удалении продукта: {ex.Message}");
	//			throw;
	//		}
	//		catch (Exception ex)
	//		{
	//			Console.WriteLine($"Ошибка при удалении продукта: {ex.Message}");
	//			throw;
	//		}
	//	}
	//}
	public class ProductService
	{
		private readonly IMongoCollection<Product> _products;

		public ProductService(IMongoDatabase database)
		{
			_products = database.GetCollection<Product>("Products");
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
			return await _products.Find(p => p.DiscountedPrice != null && p.DiscountedPrice > 0).ToListAsync();
		}

		public async Task<List<Product>> GetProductsByCategoryAsync(string category)
		{
			return await _products.Find(p => p.Category == category).ToListAsync();
		}
		public async Task<List<Product>> SearchProductsAsync(string query)
		{
			var filter = Builders<Product>.Filter.Or(
				 Builders<Product>.Filter.Regex(p => p.Name, new BsonRegularExpression(query, "i")),
				 Builders<Product>.Filter.Regex(p => p.Description, new BsonRegularExpression(query, "i")),
				 Builders<Product>.Filter.AnyStringIn(p => p.Tags, new BsonRegularExpression(query, "i"))
				 );
			return await _products.Find(filter).ToListAsync();
		}

		public async Task<List<Product>> GetTopRatedProductsAsync()
		{
			return await _products.Find(_ => true)
				  .SortByDescending(p => p.AverageRating)
				  .Limit(10) // можно убрать если надо все
				  .ToListAsync();
		}
		public async Task<List<Product>> GetProductsByTag(string tag)
		{
			// Используем $elemMatch для поиска товаров, содержащих указанный тег в массиве tags.
			var filter = Builders<Product>.Filter.ElemMatch(x => x.Tags, y => y == tag);
			return await _products.Find(filter).ToListAsync();
		}
		public async Task<Product> GetProductBySkuAsync(string sku)
		{
			return await _products.Find(p => p.SKU == sku).FirstOrDefaultAsync();
		}

		public async Task CreateProductAsync(Product product)
		{
			try
			{
				if (!string.IsNullOrEmpty(product.Id))
				{
					throw new ArgumentException("Поле Id должно быть пустым для автогенерации.");
				}
				product.CreatedAt = DateTime.UtcNow;
				product.UpdatedAt = DateTime.UtcNow;
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
		
		public async Task CreateManyProductsAsync(List<Product> products)
		{
			try
			{
				foreach (var product in products)
				{
					if (!string.IsNullOrEmpty(product.Id))
					{
						throw new ArgumentException("Поле Id должно быть пустым для автогенерации.");
					}
					product.CreatedAt = DateTime.UtcNow;
					product.UpdatedAt = DateTime.UtcNow;
				}
				await _products.InsertManyAsync(products);

			}
			catch (MongoWriteException ex)
			{
				Console.WriteLine($"Ошибка при вставке продуктов: {ex.Message}");
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при вставке продуктов: {ex.Message}");
				throw;
			}
		}

		public async Task<bool> UpdateProductAsync(string id, Product updatedProduct)
		{
			try
			{
				var existingProduct = await _products.Find(p => p.Id == id).FirstOrDefaultAsync();
				if (existingProduct == null)
					return false;
				updatedProduct.UpdatedAt = DateTime.UtcNow;
				var result = await _products.ReplaceOneAsync(p => p.Id == id, updatedProduct);
				return result.IsAcknowledged && result.ModifiedCount > 0;
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

		public async Task<bool> DeleteProductAsync(string id)
		{
			try
			{
				var result = await _products.DeleteOneAsync(p => p.Id == id);
				return result.IsAcknowledged && result.DeletedCount > 0;
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
