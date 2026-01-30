using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PosSystem.Api.Security
{
    public class RequestValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Validate request headers for potential security threats
            if (HasSecurityThreats(context.Request.Headers))
            {
                context.Response.StatusCode = 400; // Bad Request
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "Security validation failed",
                    message = "Request contains potential security threats"
                };
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            // For POST/PUT requests, validate the body content
            if (context.Request.Method == "POST" || context.Request.Method == "PUT")
            {
                var originalBody = context.Request.Body;
                
                // Allow reading the body multiple times
                context.Request.EnableBuffering();
                
                var bodyText = string.Empty;
                if (context.Request.ContentLength > 0)
                {
                    using var reader = new StreamReader(context.Request.Body);
                    bodyText = await reader.ReadToEndAsync();
                    
                    // Reset the body stream position for the next middleware/controller
                    context.Request.Body.Seek(0, SeekOrigin.Begin);
                }

                // Validate body content for potential threats
                if (!string.IsNullOrEmpty(bodyText) && HasSecurityThreatsInContent(bodyText))
                {
                    context.Response.StatusCode = 400; // Bad Request
                    context.Response.ContentType = "application/json";
                    
                    var response = new
                    {
                        error = "Security validation failed",
                        message = "Request body contains potential security threats"
                    };
                    
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    return;
                }
            }

            await _next(context);
        }

        private bool HasSecurityThreats(IHeaderDictionary headers)
        {
            var dangerousHeaders = new[]
            {
                "X-Forwarded-For",
                "X-Real-IP",
                "X-Client-IP",
                "X-Original-Forwarded-For"
            };

            foreach (var header in dangerousHeaders)
            {
                if (headers.ContainsKey(header))
                {
                    var value = headers[header].ToString();
                    if (HasSecurityThreatsInContent(value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool HasSecurityThreatsInContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return false;

            // Check for potential SQL injection patterns
            var sqlInjectionPatterns = new[]
            {
                @"\b(union|select|insert|update|delete|drop|create|alter|exec|execute|sp_|xp_|sysobjects|syscolumns)\b",
                @"(\b|\s)(and|or)(\s|\b)",
                @"';\s*--",
                @"';\s*\/\*",
                @"';\s*exec",
                @"';\s*execute",
                @"';\s*drop",
                @"';\s*create",
                @"';\s*alter",
                @"';\s*declare",
                @"';\s*open",
                @"';\s*close",
                @"';\s*fetch",
                @"';\s*kill"
            };

            foreach (var pattern in sqlInjectionPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                {
                    return true;
                }
            }

            // Check for potential XSS patterns
            var xssPatterns = new[]
            {
                @"<script[^>]*>.*?</script>",
                @"javascript:",
                @"vbscript:",
                @"on\w+\s*=",
                @"<iframe[^>]*>.*?</iframe>",
                @"<object[^>]*>.*?</object>",
                @"<embed[^>]*>.*?</embed>",
                @"<form[^>]*>.*?</form>",
                @"eval\s*\(",
                @"expression\s*\(",
                @"<link[^>]*>",
                @"<meta[^>]*>"
            };

            foreach (var pattern in xssPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                {
                    return true;
                }
            }

            // Check for potential path traversal
            if (Regex.IsMatch(content, @"\.\.\\|\.\\|\.\./|\.\/", RegexOptions.IgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}