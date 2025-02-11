namespace CyberBazaECommerce.Models
{
	public class CreateOrderDto
	{
		public List<OrderItem> OrderItems { get; set; }
		public Address ShippingAddress { get; set; }
		public string PaymentMethod { get; set; }
	}
}
