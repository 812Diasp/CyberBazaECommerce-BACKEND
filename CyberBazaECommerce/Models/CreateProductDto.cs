using System.ComponentModel.DataAnnotations;

namespace CyberBazaECommerce.Models
{
	public class CreateProductDto
	{
		//[Required]
		//public string Brand { get; set; }

		//[Required]
		//public string Category { get; set; }

		//[Required]
		//public string Name { get; set; }
		//public string Image { get; set; }
		//public string Description { get; set; }

		//public Dictionary<string, string> Characteristics { get; set; }

		//[Required]
		//public decimal Price { get; set; }

		//public decimal? DiscountedPrice { get; set; }
		
			public string Brand { get; set; }
			public string Category { get; set; }
			public string Image { get; set; }
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
			public string SKU { get; set; }
			public List<string> Tags { get; set; }
			public bool IsAvailable { get; set; }
			public string Manufacturer { get; set; }
			public decimal? ShippingCost { get; set; }
			public string Vendor { get; set; }
			public string WarrantyPeriod { get; set; }
		}
		
	}

