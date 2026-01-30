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
            // Check if the user is authenticated
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthorized access attempt to {Controller}.{Action} from IP {IP}", 
                    context.RouteData.Values["controller"], 
                    context.RouteData.Values["action"], 
                    GetClientIpAddress(context.HttpContext));

                context.Result = new UnauthorizedResult();
                return;
            }

            // Log successful authorization
            var userId = context.HttpContext.User.FindFirst("nameid")?.Value ?? "unknown";
            var username = context.HttpContext.User.FindFirst("unique_name")?.Value ?? "unknown";
            
            _logger.LogInformation("Authorization successful for user {UserId} ({Username}) accessing {Controller}.{Action}", 
                userId, 
                username, 
                context.RouteData.Values["controller"], 
                context.RouteData.Values["action"]);
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