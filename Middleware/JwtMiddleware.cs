using SecureMongoApi.Services;

namespace SecureMongoApi.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IJwtService jwtService, IAuthService authService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var userId = jwtService.ValidateToken(token);
                if (!string.IsNullOrEmpty(userId))
                {
                    context.Items["User"] = await authService.GetUserByIdAsync(userId);
                }
            }

            await _next(context);
        }
    }
}