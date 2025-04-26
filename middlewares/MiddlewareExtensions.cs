using Microsoft.AspNetCore.Builder;

namespace Backend.services.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseChatLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ChatLoggingMiddleware>();
        }
    }
}
