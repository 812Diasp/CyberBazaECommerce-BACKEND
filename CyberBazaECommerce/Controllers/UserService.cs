using CyberBazaECommerce.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CyberBazaECommerce.Controllers
{
	public class UserService
	{
		private readonly IMongoCollection<User> _users;

		public UserService(IMongoDatabase database)
		{
			_users = database.GetCollection<User>("Users");
		}
		public async Task<User> GetUserByIdAsync(string id)
		{
			return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
		}


		public async Task<User> GetUserByEmailAsync(string email)
		{
			return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
		}

		public async Task CreateUserAsync(User user)
		{
			try
			{
				if (!string.IsNullOrEmpty(user.Id))
				{
					throw new ArgumentException("Поле Id должно быть пустым для автогенерации.");
				}
				await _users.InsertOneAsync(user);
			}
			catch (MongoWriteException ex)
			{
				Console.WriteLine($"Ошибка при вставке пользователя: {ex.Message}");
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при вставке пользователя: {ex.Message}");
				throw;
			}

		}

		public async Task AddItemToCartAsync(string userId, string productId, int quantity)
		{
			try
			{
				var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
				if (user == null)
				{
					throw new ArgumentException($"User with id: {userId} not found");
				}
				var existingCartItem = user.Cart?.FirstOrDefault(item => item.ProductId == productId);
				if (existingCartItem != null)
				{
					var update = Builders<User>.Update.Set("Cart.$[elem].Quantity", existingCartItem.Quantity + quantity);
					var arrayFilters = new List<ArrayFilterDefinition> { new BsonDocumentArrayFilterDefinition<CartItem>(new BsonDocument("elem.ProductId", productId)) };
					await _users.UpdateOneAsync(u => u.Id == userId, update, new UpdateOptions { ArrayFilters = arrayFilters });

				}
				else
				{
					var cartItem = new CartItem
					{
						ProductId = productId,
						Quantity = quantity,
					};
					var update = Builders<User>.Update.Push(u => u.Cart, cartItem);
					await _users.UpdateOneAsync(u => u.Id == userId, update);
				}
			}
			catch (MongoWriteException ex)
			{
				Console.WriteLine($"Ошибка при добавлении в корзину: {ex.Message}");
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при добавлении в корзину: {ex.Message}");
				throw;
			}

		}

		public async Task RemoveItemFromCartAsync(string userId, string productId)
		{
			try
			{
				var update = Builders<User>.Update.PullFilter(u => u.Cart, Builders<CartItem>.Filter.Eq(item => item.ProductId, productId));
				await _users.UpdateOneAsync(u => u.Id == userId, update);
			}
			catch (MongoWriteException ex)
			{
				Console.WriteLine($"Ошибка при удалении из корзины: {ex.Message}");
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при удалении из корзины: {ex.Message}");
				throw;
			}
		}
		public async Task UpdateCartItemQuantityAsync(string userId, string productId, int quantity)
		{
			try
			{
				var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
				if (user == null)
				{
					throw new ArgumentException($"User with id: {userId} not found");
				}
				if (user.Cart == null)
					throw new ArgumentException("Cart is empty");
				var existingCartItem = user.Cart?.FirstOrDefault(item => item.ProductId == productId);

				if (existingCartItem == null)
					throw new ArgumentException($"product with id: {productId} not found in cart");

				var update = Builders<User>.Update.Set("Cart.$[elem].Quantity", quantity);
				var arrayFilters = new List<ArrayFilterDefinition> { new BsonDocumentArrayFilterDefinition<CartItem>(new BsonDocument("elem.ProductId", productId)) };
				await _users.UpdateOneAsync(u => u.Id == userId, update, new UpdateOptions { ArrayFilters = arrayFilters });

			}
			catch (MongoWriteException ex)
			{
				Console.WriteLine($"Ошибка при обновлении количества: {ex.Message}");
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при обновлении количества: {ex.Message}");
				throw;
			}
		}

		public async Task ClearCartAsync(string userId)
		{
			try
			{
				var update = Builders<User>.Update.Set(u => u.Cart, new List<CartItem>());
				await _users.UpdateOneAsync(u => u.Id == userId, update);
			}
			catch (MongoWriteException ex)
			{
				Console.WriteLine($"Ошибка при очистке корзины: {ex.Message}");
				throw;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка при очистке корзины: {ex.Message}");
				throw;
			}
		}
	}
}
