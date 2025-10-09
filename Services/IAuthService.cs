using SecureMongoApi.Models;

namespace SecureMongoApi.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(string username, string email, string password, List<string> roles);
        Task<(User? user, string token)> LoginAsync(string email, string password);
        Task<User?> GetUserByIdAsync(string id);
    }
}