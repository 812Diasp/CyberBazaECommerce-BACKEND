using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
namespace CyberBazaECommerce.Models
{
	public class Product
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("brand")]
		public string Brand { get; set; }

		[BsonElement("category")]
		public string Category { get; set; }

		// Основное изображение товара
		[BsonElement("mainImage")]
		public string MainImage { get; set; }

		// Массив дополнительных изображений
		[BsonElement("additionalImages")]
		public List<string> AdditionalImages { get; set; } = new List<string>();

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("description")]
		public string Description { get; set; }

		[BsonElement("characteristics")]
		public Dictionary<string, string> Characteristics { get; set; }

		[BsonElement("price")]
		public decimal Price { get; set; }

		[BsonElement("originalPrice")]
		public decimal OriginalPrice { get; set; }

		[BsonElement("discountPercentage")]
		public decimal? DiscountPercentage { get; set; }

		[BsonElement("discountedPrice")]
		public decimal? DiscountedPrice { get; set; }

		[BsonElement("discountStartDate")]
		public DateTime? DiscountStartDate { get; set; }

		[BsonElement("discountEndDate")]
		public DateTime? DiscountEndDate { get; set; }

		[BsonElement("vatRate")]
		public decimal? VatRate { get; set; }

		[BsonElement("reviews")]
		public List<Review> Reviews { get; set; } = new List<Review>();

		[BsonElement("reviewCount")]
		public int ReviewCount { get; set; }

		[BsonElement("ratingCount")]
		public int RatingCount { get; set; }

		[BsonElement("fiveStarRatingCount")]
		public int FiveStarRatingCount { get; set; }

		[BsonElement("fourStarRatingCount")]
		public int FourStarRatingCount { get; set; }

		[BsonElement("threeStarRatingCount")]
		public int ThreeStarRatingCount { get; set; }

		[BsonElement("twoStarRatingCount")]
		public int TwoStarRatingCount { get; set; }

		[BsonElement("oneStarRatingCount")]
		public int OneStarRatingCount { get; set; }

		[BsonElement("averageRating")]
		public double AverageRating { get; set; }

		[BsonElement("stockQuantity")]
		public int StockQuantity { get; set; }

		[BsonElement("weight")]
		public decimal Weight { get; set; }

		[BsonElement("dimensions")]
		public Dimensions Dimensions { get; set; }

		[BsonElement("tags")]
		public List<string> Tags { get; set; } = new List<string>();

		[BsonElement("isAvailable")]
		public bool IsAvailable { get; set; } = true;

		[BsonElement("createdAt")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[BsonElement("updatedAt")]
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		[BsonElement("variants")]
		public List<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

		[BsonElement("metaTitle")]
		public string MetaTitle { get; set; }

		[BsonElement("metaDescription")]
		public string MetaDescription { get; set; }

		[BsonElement("urlSlug")]
		public string UrlSlug { get; set; }

		[BsonElement("manufacturer")]
		public string Manufacturer { get; set; }

		[BsonElement("shippingCost")]
		public decimal? ShippingCost { get; set; }

		[BsonElement("vendor")]
		public string Vendor { get; set; }

		[BsonElement("warrantyPeriod")]
		public string WarrantyPeriod { get; set; }

		// Массив доступных цветов
		[BsonElement("colors")]
		public List<string> Colors { get; set; } = new List<string>();

		// Опции товара (например, размер памяти)
		[BsonElement("options")]
		public List<Option> Options { get; set; } = new List<Option>();
	}

	public class Option
	{
		[BsonElement("name")]
		public string Name { get; set; } // Название опции (например, "Размер памяти")

		[BsonElement("values")]
		public List<string> Values { get; set; } // Доступные значения (например, "64GB", "128GB")
	}

	public class ProductVariant
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("options")]
		public Dictionary<string, string> Options { get; set; }

		[BsonElement("image")]
		public string Image { get; set; }

		[BsonElement("stockQuantity")]
		public int StockQuantity { get; set; }

		[BsonElement("price")]
		public decimal Price { get; set; }
	}
}