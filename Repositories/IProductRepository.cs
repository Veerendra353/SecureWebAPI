using SecureMongoApi.Models;

namespace SecureMongoApi.Repositories
{
    public interface IProductRepository
    {
        Task<Product> GetByIdAsync(string id);
        Task<List<Product>> GetAllAsync();
        Task<Product> CreateAsync(Product product);
        Task<Product> UpdateAsync(string id, Product product);
        Task<bool> DeleteAsync(string id);
        Task<List<Product>> GetByCategoryAsync(string category);
    }
}