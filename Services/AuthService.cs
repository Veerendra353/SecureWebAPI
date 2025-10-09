using MongoDB.Driver;
using SecureMongoApi.Data;
using SecureMongoApi.Models;

namespace SecureMongoApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly MongoDbContext _context;
        private readonly IJwtService _jwtService;

        public AuthService(MongoDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        public async Task<User> RegisterAsync(string username, string email, string password, List<string> roles)
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .Find(u => u.Email == email || u.Username == username)
                .FirstOrDefaultAsync();

            if (existingUser != null)
                throw new InvalidOperationException("User already exists");

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Roles = roles,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.InsertOneAsync(user);
            return user;
        }

        public async Task<(User? user, string token)> LoginAsync(string email, string password)
        {
            var user = await _context.Users
                .Find(u => u.Email == email && u.IsActive)
                .FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return (null, string.Empty);

            var token = _jwtService.GenerateToken(user);
            return (user, token);
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _context.Users
                .Find(u => u.Id == id && u.IsActive)
                .FirstOrDefaultAsync();
        }
    }
}