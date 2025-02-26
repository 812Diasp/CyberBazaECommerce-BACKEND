﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace CyberBazaECommerce.Models
{
	public class User
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("Username")]
		public string Username { get; set; }

		[BsonElement("PasswordHash")]
		public string PasswordHash { get; set; }

		[BsonElement("Email")]
		public string Email { get; set; }
		[BsonElement("Role")] // Добавляем поле для хранения роли
		public string Role { get; set; } = "customer"; // Set default role to customer
		[BsonElement("Favorites")]
		public List<string> Favorites { get; set; } = new List<string>();

		[BsonElement("Tracking")]
		public List<string> Tracking { get; set; } = new List<string>();
		[BsonElement("Cart")]
		public List<CartItem> Cart { get; set; } = new List<CartItem>();
		[BsonElement("ConfirmationCode")]
		public string ConfirmationCode { get; set; }
		[BsonElement("ConfirmationCodeExpiration")]
		public DateTime ConfirmationCodeExpiration { get; set; }
	}
	public class CartItem
	{
		public string ProductId { get; set; } // ID товара
		public int Quantity { get; set; } // Количество
		public string Color { get; set; } // Добавлено поле для цвета
		public string Variant { get; set; } // Добавлено поле для варианта
	}
}

