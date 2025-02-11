using CyberBazaECommerce.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyberBazaECommerce.Services
{


public class OrderService : IOrderService
{

		private readonly IMongoCollection<Order> _orders;
		private readonly IMongoCollection<Product> _products;

		public OrderService(IMongoDatabase database)
		{
			_orders = database.GetCollection<Order>("Orders");
			_products = database.GetCollection<Product>("Products");
		}

		public async Task<Order> CreateOrderAsync(Order order)
		{
			order.TotalPrice = 0; // Обнуляем общую стоимость заказа перед расчётом
			foreach (var item in order.OrderItems)
			{
				var product = await _products.Find(p => p.Id == item.ProductId).FirstOrDefaultAsync();
				if (product == null)
				{
					throw new Exception($"Product with ID '{item.ProductId}' not found.");
				}

				order.TotalPrice += product.Price * item.Quantity;
			}

			order.OrderDate = DateTime.UtcNow;
			await _orders.InsertOneAsync(order);
			return order;
		}

		public async Task<Order> GetOrderByIdAsync(string orderId)
		{
			return await _orders.Find(o => o.Id == orderId).FirstOrDefaultAsync();
		}
		public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
		{
			return await _orders.Find(o => o.UserId == userId).ToListAsync();
		}
		public async Task UpdateOrderStatusAsync(string orderId, string newStatus)
		{
			var update = Builders<Order>.Update.Set(o => o.OrderStatus, newStatus);
			await _orders.UpdateOneAsync(o => o.Id == orderId, update);
		}
	}
}
