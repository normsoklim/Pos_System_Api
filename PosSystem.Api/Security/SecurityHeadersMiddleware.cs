using Microsoft.AspNetCore.Http;

namespace PosSystem.Api.Security
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Add security headers to the response
            context.Response.OnStarting(() =>
            {
                // Prevent MIME type sniffing
                if (!context.Response.Headers.ContainsKey("X-Content-Type-Options"))
                {
                    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                }

                // Prevent clickjacking
                if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
                {
                    context.Response.Headers.Append("X-Frame-Options", "DENY");
                }

                // Enable XSS protection in browsers
                if (!context.Response.Headers.ContainsKey("X-XSS-Protection"))
                {
                    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                }

                // Prevent content-type sniffing for scripts
                if (!context.Response.Headers.ContainsKey("X-Permitted-Cross-Domain-Policies"))
                {
                    context.Response.Headers.Append("X-Permitted-Cross-Domain-Policies", "none");
                }

                // Set Content Security Policy
                if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
                {
                    context.Response.Headers.Append("Content-Security-Policy",
                        "default-src 'self'; " +
                        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
                        "style-src 'self' 'unsafe-inline'; " +
                        "img-src 'self' data: https:; " +
                        "font-src 'self' data:; " +
                        "connect-src 'self'; " +
                        "frame-ancestors 'none'; " +
                        "object-src 'none'; " +
                        "base-uri 'self';");
                }

                // Strict Transport Security (HSTS) - only if HTTPS is available
                if (!context.Response.Headers.ContainsKey("Strict-Transport-Security") && context.Request.IsHttps)
                {
                    context.Response.Headers.Append("Strict-Transport-Security",
                        "max-age=31536000; includeSubDomains; preload");
                }

                // Referrer Policy
                if (!context.Response.Headers.ContainsKey("Referrer-Policy"))
                {
                    context.Response.Headers.Append("Referrer-Policy", "no-referrer-when-downgrade");
                }

                // Feature Policy (now called Permissions Policy)
                if (!context.Response.Headers.ContainsKey("Permissions-Policy"))
                {
                    context.Response.Headers.Append("Permissions-Policy",
                        "geolocation=(), microphone=(), camera=()");
                }

                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}