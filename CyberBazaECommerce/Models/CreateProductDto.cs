using System.ComponentModel.DataAnnotations;

namespace CyberBazaECommerce.Models
{
	public class CreateProductDto
	{
		[Required]
		public string Brand { get; set; }

		[Required]
		public string Category { get; set; }

		[Required]
		public string Name { get; set; }

		public string Description { get; set; }

		public Dictionary<string, string> Characteristics { get; set; }

		[Required]
		public decimal Price { get; set; }

		public decimal? DiscountedPrice { get; set; }
	}
}
