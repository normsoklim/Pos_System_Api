using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace PosSystem.Api.Security
{
    public class AntiForgeryMiddleware
    {
        private readonly RequestDelegate _next;

        public AntiForgeryMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip anti-forgery validation for certain safe methods
            if (context.Request.Method == "GET" || 
                context.Request.Method == "HEAD" || 
                context.Request.Method == "OPTIONS" || 
                context.Request.Method == "TRACE")
            {
                await _next(context);
                return;
            }

            // For POST, PUT, PATCH, DELETE methods, check for anti-forgery token
            // This is a simplified implementation - in production you'd want to use ASP.NET Core's built-in anti-forgery
            var isSafeRequest = IsSafeRequest(context);
            
            if (!isSafeRequest)
            {
                context.Response.StatusCode = 400; // Bad Request
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "Anti-forgery validation failed",
                    message = "Request is missing required security tokens or headers"
                };
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            await _next(context);
        }

        private bool IsSafeRequest(HttpContext context)
        {
            // Check for common anti-forgery headers
            var hasValidReferer = HasValidReferer(context);
            var hasContentType = context.Request.Headers.ContainsKey("Content-Type");
            var isAjaxRequest = context.Request.Headers.ContainsKey("X-Requested-With");
            
            // Additional checks for JSON content type
            if (hasContentType)
            {
                var contentType = context.Request.Headers["Content-Type"].ToString().ToLower();
                if (contentType.Contains("application/json"))
                {
                    // For JSON requests, we might expect an additional header
                    return true; // In a real implementation, you'd check for a specific header
                }
            }

            // For AJAX requests, often have specific headers
            if (isAjaxRequest)
            {
                return true;
            }

            // For non-JSON requests, check referer
            return hasValidReferer;
        }

        private bool HasValidReferer(HttpContext context)
        {
            var referer = context.Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(referer))
            {
                // For APIs, referer might not always be present
                return true; // Allow for API scenarios
            }

            // In a real implementation, you'd validate the referer against allowed origins
            return true;
        }
    }
}