using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SecureMongoApi.Models;
using SecureMongoApi.Models.Configurations;

namespace SecureMongoApi.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Product> Products => _database.GetCollection<Product>("Products");
    }
}