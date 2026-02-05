using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PosSystem.Infrastructure.Persistence;

namespace PosSystem.Api.Security
{
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKeyHeaderName;
        private readonly string? _globalApiKey;

        public ApiKeyAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _apiKeyHeaderName = configuration["ApiKey:HeaderName"] ?? "X-API-Key";
            _globalApiKey = configuration["ApiKey:Key"];
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip API key validation for certain paths (like auth endpoints) or GET requests to specific endpoints
            if (ShouldSkipApiKeyValidation(context.Request.Path, context.Request.Method))
            {
                await _next(context);
                return;
            }

            // Check if user is already authenticated via JWT - if so, skip API key validation
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(_apiKeyHeaderName, out var extractedApiKey))
            {
                // If no API key is provided, continue to next middleware (JWT will handle authentication)
                await _next(context);
                return;
            }

            // First check if it's the global API key
            if (!string.IsNullOrEmpty(_globalApiKey) && extractedApiKey == _globalApiKey)
            {
                // Global API key is valid, continue with basic API key identity
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "API-Client"),
                    new Claim("api_key_authenticated", "true"),
                    new Claim(ClaimTypes.Role, "User") // Default role for API key users
                };

                var identity = new ClaimsIdentity(claims, "API-Key");
                var principal = new ClaimsPrincipal(identity);
                context.User = principal;
                
                await _next(context);
                return;
            }

            // If API key is provided but invalid, return unauthorized
            context.Response.StatusCode = 401; // Unauthorized
            context.Response.ContentType = "application/json";
            
            var invalidKeyResponse = new
            {
                error = "Invalid API Key",
                message = "The provided API Key is invalid."
            };
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(invalidKeyResponse));
            return;
        }

        private bool ShouldSkipApiKeyValidation(string path, string method)
        {
            // Skip API key validation for specific endpoints
            var pathLower = path.ToLowerInvariant();
            var skipPaths = new[]
            {
                "/swagger",
                "/swagger/",
                "/swagger/index.html",
                "/api/test/public",
                "/health",
                "/api/auth"  // Skip API key validation for auth endpoints
            };

            // Skip API key validation for GET requests to category and product endpoints (these should be public)
            if ((pathLower.StartsWith("/api/category") || pathLower.StartsWith("/api/product")) && method.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return skipPaths.Any(skipPath => pathLower.StartsWith(skipPath.ToLowerInvariant()));
        }
    }
}