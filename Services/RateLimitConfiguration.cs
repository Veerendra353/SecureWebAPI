using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace SecureMongoApi.Services
{
    public static class RateLimitConfiguration
    {
        public static void ConfigureRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("Fixed", opt =>
                {
                    opt.PermitLimit = configuration.GetValue<int>("RateLimiting:PermitLimit", 1000);
                    opt.Window = TimeSpan.FromSeconds(configuration.GetValue<int>("RateLimiting:Window", 10));
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = configuration.GetValue<int>("RateLimiting:QueueLimit", 100);
                });
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
                };
            });
        }
    }
}