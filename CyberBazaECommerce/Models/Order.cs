using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
namespace CyberBazaECommerce.Models
{
	// Order.cs
	using MongoDB.Bson;
	using MongoDB.Bson.Serialization.Attributes;
	using System;
	using System.Collections.Generic;

	public class Order
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string Id { get; set; }

		[BsonElement("UserId")]
		public string UserId { get; set; }

		[BsonElement("OrderItems")]
		public List<OrderItem> OrderItems { get; set; }

		[BsonElement("TotalPrice")]
		public decimal TotalPrice { get; set; }

		[BsonElement("OrderDate")]
		public DateTime OrderDate { get; set; }

		[BsonElement("OrderStatus")]
		public string OrderStatus { get; set; } = "Pending";

		[BsonElement("ShippingAddress")]
		public Address ShippingAddress { get; set; }

		[BsonElement("PaymentMethod")]
		public string PaymentMethod { get; set; }
	}

// OrderItem.cs


public class OrderItem
	{
		[BsonElement("ProductId")]
		public string ProductId { get; set; }

		[BsonElement("Quantity")]
		public int Quantity { get; set; }
	}


public class Address
	{
		[BsonElement("Street")]
		public string Street { get; set; }
		[BsonElement("City")]
		public string City { get; set; }
		[BsonElement("State")]
		public string State { get; set; }
		[BsonElement("PostalCode")]
		public string PostalCode { get; set; }

	}
}
