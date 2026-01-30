using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text.Json;

namespace PosSystem.Api.Security
{
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _apiKeyHeaderName;
        private readonly string _apiKey;

        public ApiKeyAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _apiKeyHeaderName = configuration["ApiKey:HeaderName"] ?? "X-API-Key";
            _apiKey = configuration["ApiKey:Key"];
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip API key validation for certain paths (like auth endpoints)
            if (ShouldSkipApiKeyValidation(context.Request.Path))
            {
                await _next(context);
                return;
            }

            if (string.IsNullOrEmpty(_apiKey))
            {
                // If no API key is configured, skip validation
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(_apiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "API Key is missing",
                    message = "API Key is required for this endpoint"
                };
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            if (extractedApiKey != _apiKey)
            {
                context.Response.StatusCode = 401; // Unauthorized
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "Invalid API Key",
                    message = "The provided API Key is invalid"
                };
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            // Add API key to claims if needed
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                // If user is already authenticated via JWT, add API key info to claims
                var identity = context.User.Identity as ClaimsIdentity;
                identity?.AddClaim(new Claim("api_key_authenticated", "true"));
            }
            else
            {
                // Create a basic identity with API key authentication
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, "API-Client"),
                    new Claim("api_key_authenticated", "true"),
                    new Claim(ClaimTypes.Role, "API-Client")
                };

                var identity = new ClaimsIdentity(claims, "API-Key");
                var principal = new ClaimsPrincipal(identity);
                context.User = principal;
            }

            await _next(context);
        }

        private bool ShouldSkipApiKeyValidation(string path)
        {
            // Skip API key validation for authentication endpoints
            var skipPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/refresh",
                "/swagger",
                "/swagger/",
                "/swagger/index.html",
                "/api/test/public"
            };

            return skipPaths.Any(skipPath => path.ToString().StartsWith(skipPath, StringComparison.OrdinalIgnoreCase));
        }
    }
}