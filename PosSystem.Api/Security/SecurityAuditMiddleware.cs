using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PosSystem.Api.Security
{
    public class SecurityAuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SecurityAuditMiddleware> _logger;

        public SecurityAuditMiddleware(RequestDelegate next, ILogger<SecurityAuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            var requestTime = DateTime.UtcNow;
            var clientIp = GetClientIpAddress(context);
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var method = context.Request.Method;
            var path = context.Request.Path;
            var queryString = context.Request.QueryString.ToString();

            try
            {
                await _next(context);

                // Log successful requests
                if (context.Response.StatusCode < 400)
                {
                    _logger.LogInformation(
                        "SECURITY_AUDIT: Successful request - IP: {ClientIp}, Method: {Method}, Path: {Path}, User: {UserId}, Status: {StatusCode}, Duration: {Duration}ms",
                        clientIp,
                        method,
                        path,
                        GetUserId(context),
                        context.Response.StatusCode,
                        (DateTime.UtcNow - requestTime).TotalMilliseconds
                    );
                }
                else
                {
                    // Log potentially suspicious requests
                    _logger.LogWarning(
                        "SECURITY_AUDIT: Request with error - IP: {ClientIp}, Method: {Method}, Path: {Path}, User: {UserId}, Status: {StatusCode}, Duration: {Duration}ms",
                        clientIp,
                        method,
                        path,
                        GetUserId(context),
                        context.Response.StatusCode,
                        (DateTime.UtcNow - requestTime).TotalMilliseconds
                    );
                }
            }
            catch (Exception ex)
            {
                // Log exceptions
                _logger.LogError(
                    ex,
                    "SECURITY_AUDIT: Exception occurred - IP: {ClientIp}, Method: {Method}, Path: {Path}, User: {UserId}, Exception: {ExceptionMessage}",
                    clientIp,
                    method,
                    path,
                    GetUserId(context),
                    ex.Message
                );

                throw;
            }
            finally
            {
                await responseBody.CopyToAsync(originalBodyStream);
            }
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

        private string GetUserId(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst("nameid")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    return userId;
                }

                var username = context.User.FindFirst("unique_name")?.Value;
                if (!string.IsNullOrEmpty(username))
                {
                    return username;
                }
            }

            return "anonymous";
        }
    }
}