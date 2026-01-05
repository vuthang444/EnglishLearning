using Microsoft.AspNetCore.Authentication;

namespace EnglishLearning.Middleware
{
    public class JwtCookieMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtCookieMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Đọc token từ cookie
            var token = context.Request.Cookies["AuthToken"];

            if (!string.IsNullOrEmpty(token) && !context.Request.Headers.ContainsKey("Authorization"))
            {
                // Thêm token vào header Authorization để JWT middleware có thể xử lý
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            }

            await _next(context);
        }
    }

    public static class JwtCookieMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtCookie(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtCookieMiddleware>();
        }
    }
}

