using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace PosSystem.Api.Filters
{
    public class CustomAuthorizationFilter : IAuthorizationFilter
    {
        private readonly ILogger<CustomAuthorizationFilter> _logger;

        public CustomAuthorizationFilter(ILogger<CustomAuthorizationFilter> logger)
        {
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            
            // Check if user is authenticated
            if (user?.Identity?.IsAuthenticated != true)
            {
                _logger.LogWarning("Unauthorized access attempt from IP: {IP}", GetClientIpAddress(context.HttpContext));
                context.Result = new UnauthorizedResult();
                return;
            }

            // Log successful authentication
            var userId = user.FindFirst("name")?.Value ?? "unknown";
            var apiKeyAuthenticated = user.FindFirst("api_key_authenticated")?.Value ?? "false";
            
            _logger.LogInformation("User {UserId} with API key authentication status {ApiKeyStatus} accessed {Path}",
                userId, apiKeyAuthenticated, context.HttpContext.Request.Path);
        }

        private string GetClientIpAddress(HttpContext context)
        {
            // Check for forwarded headers first (for reverse proxies/load balancers)
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fallback to connection remote IP
            return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}