using CyberBazaECommerce.Models;

namespace CyberBazaECommerce.Services
{
	// IOrderService.cs
	public interface IOrderService
	{
		Task<Order> CreateOrderAsync(Order order);
		Task<Order> GetOrderByIdAsync(string orderId);
		Task<List<Order>> GetOrdersByUserIdAsync(string userId);
		Task UpdateOrderStatusAsync(string orderId, string newStatus);
	}
}
