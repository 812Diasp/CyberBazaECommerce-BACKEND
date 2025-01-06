using CyberBazaECommerce.Models;
using MongoDB.Driver;

namespace CyberBazaECommerce.Controllers
{
	public interface IReviewService
	{
		Task AddReviewAsync(string productId, Review review);
		Task<List<Review>> GetReviewsByProductIdAsync(string productId);
		Task UpdateProductRatingAsync(string productId);
	}

	public class ReviewService : IReviewService
	{
		private readonly ILogger<ReviewService> _logger;
		private readonly IMongoCollection<Product> _products;
		private readonly IMongoCollection<Review> _reviews;

		public ReviewService(IMongoDatabase database, ILogger<ReviewService> logger)
		{
			_products = database.GetCollection<Product>("Products");
			_reviews = database.GetCollection<Review>("Reviews");
			_logger = logger;
		}

		public async Task AddReviewAsync(string productId, Review review)
		{
			review.Date = DateTime.UtcNow;
			await _reviews.InsertOneAsync(review);
			_logger.LogInformation($"Review added to collection: {review.Id}");

			var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();

			if (product != null)
			{
				var update = Builders<Product>.Update.Push(p => p.Reviews, review);

				await _products.UpdateOneAsync(p => p.Id == productId, update);
				_logger.LogInformation($"Review added to product: {product.Id}, review count: {product.Reviews.Count}");
				await UpdateProductRatingAsync(productId);
			}
			else
			{
				_logger.LogWarning($"Product with id: {productId} not found");
			}
		}
		public async Task UpdateProductRatingAsync(string productId)
		{
			var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();

			if (product != null)
			{
				var update = Builders<Product>.Update;

				UpdateDefinition<Product> updateDefinition;

				if (product.Reviews != null && product.Reviews.Any())
				{
					var reviewCount = product.Reviews.Count;
					var averageRating = Math.Round(product.Reviews.Average(r => r.Rating), 1);
					updateDefinition = update.Set(p => p.ReviewCount, reviewCount)
							.Set(p => p.AverageRating, averageRating);
					_logger.LogInformation($"Product updated: {product.Id}, reviewCount: {reviewCount}, averageRating: {averageRating}");
				}
				else
				{
					updateDefinition = update
							.Set(p => p.ReviewCount, 0)
						   .Set(p => p.AverageRating, 0);
					_logger.LogInformation($"Product updated: {product.Id}, reviewCount: 0, averageRating: 0");
				}
				await _products.UpdateOneAsync(p => p.Id == productId, updateDefinition);
				_logger.LogInformation($"Product updated in db: {product.Id}");

			}
			else
			{
				_logger.LogWarning($"Product with id: {productId} not found when updating rating");
			}
		}
		public async Task<List<Review>> GetReviewsByProductIdAsync(string productId)
		{
			var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();

			return product?.Reviews;
		}
		
	}
}
