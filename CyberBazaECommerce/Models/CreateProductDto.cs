using System.ComponentModel.DataAnnotations;

namespace CyberBazaECommerce.Models
{
	public class CreateProductDto
	{
		public string Brand { get; set; }
		public string Category { get; set; }

		// Основное изображение товара
		public string MainImage { get; set; }

		// Дополнительные изображения товара
		public List<string> AdditionalImages { get; set; }

		public string Name { get; set; }
		public string Description { get; set; }
		public Dictionary<string, string> Characteristics { get; set; }
		public decimal Price { get; set; }
		public decimal? OriginalPrice { get; set; }
		public decimal? DiscountPercentage { get; set; }
		public decimal? DiscountedPrice { get; set; }
		public DateTime? DiscountStartDate { get; set; }
		public DateTime? DiscountEndDate { get; set; }
		public decimal? VatRate { get; set; }
		public int StockQuantity { get; set; }
		public decimal Weight { get; set; }
		public Dimensions Dimensions { get; set; }

		public List<string> Tags { get; set; }
		public bool IsAvailable { get; set; }
		public string Manufacturer { get; set; }
		public decimal? ShippingCost { get; set; }
		public string Vendor { get; set; }
		public string WarrantyPeriod { get; set; }

		// Массив цветов товара
		public List<string> Colors { get; set; }

		// Опции товара (например, размер памяти)
		public List<Option> Options { get; set; }
	}

}