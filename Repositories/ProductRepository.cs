using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SecureMongoApi.Data;
using SecureMongoApi.Models;
using SecureMongoApi.Models.Configurations;
using System.Text.Json;

namespace SecureMongoApi.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly MongoDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly RedisSettings _redisSettings;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public ProductRepository(MongoDbContext context, IDistributedCache cache, IOptions<RedisSettings> redisSettings)
        {
            _context = context;
            _cache = cache;
            _redisSettings = redisSettings.Value;
        }

        private string GetCacheKey(string key) => $"{_redisSettings.InstanceName}product:{key}";

        public async Task<Product> GetByIdAsync(string id)
        {
            var cacheKey = GetCacheKey($"id:{id}");

            // Try to get from cache
            var cachedProduct = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedProduct))
            {
                return JsonSerializer.Deserialize<Product>(cachedProduct)!;
            }

            // Get from database
            var product = await _context.Products
                .Find(p => p.Id == id && p.IsActive)
                .FirstOrDefaultAsync();

            if (product != null)
            {
                // Cache the result
                await _cache.SetStringAsync(cacheKey,
                    JsonSerializer.Serialize(product),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheExpiration });
            }

            return product;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var cacheKey = GetCacheKey("all");

            var cachedProducts = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedProducts))
            {
                return JsonSerializer.Deserialize<List<Product>>(cachedProducts)!;
            }

            var products = await _context.Products
                .Find(p => p.IsActive)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();

            await _cache.SetStringAsync(cacheKey,
                JsonSerializer.Serialize(products),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

            return products;
        }

        public async Task<Product> CreateAsync(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.Products.InsertOneAsync(product);

            // Invalidate relevant caches
            await InvalidateCaches();

            return product;
        }

        public async Task<Product> UpdateAsync(string id, Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;

            var result = await _context.Products.ReplaceOneAsync(
                p => p.Id == id && p.IsActive, product);

            if (result.MatchedCount == 0)
                throw new KeyNotFoundException("Product not found");

            // Invalidate caches
            await InvalidateCaches();
            await _cache.RemoveAsync(GetCacheKey($"id:{id}"));

            return product;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var update = Builders<Product>.Update
                .Set(p => p.IsActive, false)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Products.UpdateOneAsync(
                p => p.Id == id && p.IsActive, update);

            if (result.MatchedCount == 0)
                return false;

            // Invalidate caches
            await InvalidateCaches();
            await _cache.RemoveAsync(GetCacheKey($"id:{id}"));

            return true;
        }

        public async Task<List<Product>> GetByCategoryAsync(string category)
        {
            var cacheKey = GetCacheKey($"category:{category}");

            var cachedProducts = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedProducts))
            {
                return JsonSerializer.Deserialize<List<Product>>(cachedProducts)!;
            }

            var products = await _context.Products
                .Find(p => p.Category == category && p.IsActive)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();

            await _cache.SetStringAsync(cacheKey,
                JsonSerializer.Serialize(products),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheExpiration });

            return products;
        }

        private async Task InvalidateCaches()
        {
            await _cache.RemoveAsync(GetCacheKey("all"));
            // You can add more cache keys to invalidate here
        }
    }
}