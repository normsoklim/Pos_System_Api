using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Text.Json;

namespace PosSystem.Api.Security
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ConcurrentDictionary<string, List<DateTime>> _requestCounts;
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitingMiddleware(RequestDelegate next, int maxRequests = 100, TimeSpan? timeWindow = null)
        {
            _next = next;
            _requestCounts = new ConcurrentDictionary<string, List<DateTime>>();
            _maxRequests = maxRequests;
            _timeWindow = timeWindow ?? TimeSpan.FromMinutes(1);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientIp = GetClientIpAddress(context);
            var now = DateTime.UtcNow;

            // Clean up old requests outside the time window
            var requests = _requestCounts.GetOrAdd(clientIp, _ => new List<DateTime>());
            bool isRateLimited;
            
            lock (requests)
            {
                requests.RemoveAll(r => r < now.Add(_timeWindow.Negate()));
                
                if (requests.Count >= _maxRequests)
                {
                    isRateLimited = true;
                }
                else
                {
                    requests.Add(now);
                    isRateLimited = false;
                }
            }

            if (isRateLimited)
            {
                context.Response.StatusCode = 429; // Too Many Requests
                context.Response.ContentType = "application/json";
                
                var response = new
                {
                    error = "Rate limit exceeded",
                    message = $"Too many requests. Maximum {_maxRequests} requests per {_timeWindow.TotalMinutes} minutes allowed."
                };
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            await _next(context);
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