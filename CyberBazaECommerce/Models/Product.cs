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

		[BsonElement("image")]
		public string Image { get; set; }

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("description")]
		public string Description { get; set; }

		[BsonElement("characteristics")]
		public Dictionary<string, string> Characteristics { get; set; }

		[BsonElement("price")]
		public decimal Price { get; set; }

		[BsonElement("discountedPrice")]
		public decimal? DiscountedPrice { get; set; }

		[BsonElement("reviews")]
		public List<Review> Reviews { get; set; } = new List<Review>(); // Список отзывов

		[BsonElement("reviewCount")]
		public int ReviewCount { get; set; } // Количество отзывов

		[BsonElement("averageRating")]
		public double AverageRating { get; set; } // Средняя оценка
	}
}
