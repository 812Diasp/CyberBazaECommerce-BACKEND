using CyberBazaECommerce.Models;
using MongoDB.Driver;
namespace CyberBazaECommerce.Services
{
	public class MongoDbService
	{
		private readonly IMongoCollection<User> _usersCollection;

		public MongoDbService(IConfiguration config)
		{
			var connectionString = config.GetSection("MongoDb")["ConnectionString"];
			var databaseName = config.GetSection("MongoDb")["DatabaseName"];

			var client = new MongoClient(connectionString);
			var database = client.GetDatabase(databaseName);
			_usersCollection = database.GetCollection<User>("Users");
		}

		public async Task<User> GetUserByUsernameAsync(string username)
		{
			return await _usersCollection.Find(user => user.Username == username).FirstOrDefaultAsync();
		}

		public async Task<User> GetUserByEmailAsync(string email)
		{
			return await _usersCollection.Find(user => user.Email == email).FirstOrDefaultAsync();
		}

		public async Task CreateUserAsync(User user)
		{
			await _usersCollection.InsertOneAsync(user);
		}
		public async Task UpdateUserAsync(User user)
		{
			var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);
			await _usersCollection.ReplaceOneAsync(filter, user);
		}
	}
}
