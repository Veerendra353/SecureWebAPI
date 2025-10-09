using SecureMongoApi.Models;

namespace SecureMongoApi.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string? ValidateToken(string token);
    }
}