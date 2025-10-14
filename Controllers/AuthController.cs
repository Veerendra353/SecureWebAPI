using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SecureMongoApi.Filters;
using SecureMongoApi.Models;
using SecureMongoApi.Services;

namespace SecureMongoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    [EnableRateLimiting("Fixed")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [ValidateModel]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authService.RegisterAsync(
                    request.Username,
                    request.Email,
                    request.Password,
                    request.Roles);

                _logger.LogInformation("User Registered Successfully: {Email}", request.Email);

                return Ok(new { Message = "User Registered Successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (user, token) = await _authService.LoginAsync(request.Email, request.Password);

            if (user == null)
                return Unauthorized(new { Message = "Invalid Credentials" });

            _logger.LogInformation("User logged in: {Email}", request.Email);

            return Ok(new
            {
                Token = token,
                User = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Roles
                }
            });
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string> { "User" };
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}