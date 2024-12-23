using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace CyberBazaECommerce.Models
{
	public class Product
	{
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("brand")]
		public string Brand { get; set; }

		[BsonElement("category")]
		public string Category { get; set; }

		[BsonElement("name")]
		public string Name { get; set; }

		[BsonElement("description")]
		public string Description { get; set; }

		[BsonElement("characteristics")]
		public Dictionary<string, string> Characteristics { get; set; }

		[BsonElement("price")]
		public decimal Price { get; set; }

		[BsonElement("discountedPrice")]
		public decimal? DiscountedPrice { get; set; } // Nullable, если нет скидки
	}
}
