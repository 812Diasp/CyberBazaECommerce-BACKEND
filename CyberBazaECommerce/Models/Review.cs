using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace CyberBazaECommerce.Models
{
	public class Review
	{
		[BsonRepresentation(BsonType.ObjectId)]
		[JsonIgnore]
		public string Id { get; set; }

		[BsonElement("userId")]
		[JsonIgnore]
		public string UserId { get; set; }

		[BsonElement("productId")] // Добавляем productId
		public string ProductId { get; set; }

		[BsonElement("title")]
		public string Title { get; set; }

		[BsonElement("pros")]
		public string Pros { get; set; }

		[BsonElement("cons")]
		public string Cons { get; set; }

		[BsonElement("comment")]
		public string Comment { get; set; }

		[BsonElement("rating")]
		public int Rating { get; set; } // Оценка от 1 до 5, например

		[BsonElement("date")]
		[JsonIgnore]
		public DateTime Date { get; set; } // Дата отзыва
	}
}
