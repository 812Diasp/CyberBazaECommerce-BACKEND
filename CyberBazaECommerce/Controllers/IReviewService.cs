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
			var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();

			if (product == null)
			{
				_logger.LogWarning($"Product with id: {productId} not found");
				throw new ArgumentException($"Product with id: {productId} not found");
			}
			// Проверка, что пользователь уже оставлял отзыв для этого продукта
			if (product.Reviews != null && product.Reviews.Any(r => r.UserId == review.UserId))
			{
				_logger.LogWarning($"User with id: {review.UserId} already left a review for product with id: {productId}");
				throw new InvalidOperationException($"User with id: {review.UserId} already left a review for product with id: {productId}");
			}

			review.Date = DateTime.UtcNow;
			await _reviews.InsertOneAsync(review);
			_logger.LogInformation($"Review added to collection: {review.Id}");

			var update = Builders<Product>.Update.Push(p => p.Reviews, review);

			await _products.UpdateOneAsync(p => p.Id == productId, update);
			_logger.LogInformation($"Review added to product: {product.Id}, review count: {product.Reviews.Count}");
			await UpdateProductRatingAsync(productId);
		}

		public async Task UpdateProductRatingAsync(string productId)
		{
			var product = await _products.Find(p => p.Id == productId).FirstOrDefaultAsync();

			if (product != null)
			{
				var reviews = product.Reviews;
				int reviewCount = reviews.Count;
				double averageRating = 0;

				// Подсчет звездных отзывов
				int fiveStarCount = reviews.Count(r => r.Rating == 5);
				int fourStarCount = reviews.Count(r => r.Rating == 4);
				int threeStarCount = reviews.Count(r => r.Rating == 3);
				int twoStarCount = reviews.Count(r => r.Rating == 2);
				int oneStarCount = reviews.Count(r => r.Rating == 1);

				//Вычисление среднего рейтинга
				if (reviewCount > 0)
				{
					averageRating = Math.Round(reviews.Average(r => r.Rating), 1);
				}

				var update = Builders<Product>.Update
					.Set(p => p.ReviewCount, reviewCount)
					.Set(p => p.AverageRating, averageRating)
					.Set(p => p.FiveStarRatingCount, fiveStarCount)
					.Set(p => p.FourStarRatingCount, fourStarCount)
					.Set(p => p.ThreeStarRatingCount, threeStarCount)
					.Set(p => p.TwoStarRatingCount, twoStarCount)
					.Set(p => p.OneStarRatingCount, oneStarCount);

				await _products.UpdateOneAsync(p => p.Id == productId, update);
				_logger.LogInformation($"Product updated: {product.Id}, reviewCount: {reviewCount}, averageRating: {averageRating}");

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
