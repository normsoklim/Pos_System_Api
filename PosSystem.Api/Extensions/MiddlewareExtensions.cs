using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using PosSystem.Api.Security;

namespace PosSystem.Api.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseSecurityHeadersMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Security.SecurityHeadersMiddleware>();
        }

        public static IApplicationBuilder UseRequestValidationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Security.RequestValidationMiddleware>();
        }

        public static IApplicationBuilder UseApiKeyAuthenticationMiddleware(this IApplicationBuilder builder, IConfiguration configuration)
        {
            return builder.UseMiddleware<Security.ApiKeyAuthenticationMiddleware>(configuration);
        }

        public static IApplicationBuilder UseSecurityAuditMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Security.SecurityAuditMiddleware>();
        }
        
        public static IApplicationBuilder UseAntiForgeryMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Security.AntiForgeryMiddleware>();
        }
        
        public static IApplicationBuilder UseRequestSizeLimitingMiddleware(this IApplicationBuilder builder, long maxRequestBodySize = 10 * 1024 * 1024)
        {
            return builder.UseMiddleware<Security.RequestSizeLimitingMiddleware>(maxRequestBodySize);
        }
        
         public static IApplicationBuilder UseRequestBodyBufferingMiddleware(this IApplicationBuilder builder)
         {
             return builder.UseMiddleware<Security.RequestBodyBufferingMiddleware>();
         }
         
         public static IApplicationBuilder UseExceptionMiddleware(this IApplicationBuilder builder)
         {
             return builder.UseMiddleware<Middleware.ExceptionMiddleware>();
         }
    }
}