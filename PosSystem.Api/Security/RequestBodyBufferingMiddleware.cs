using Microsoft.AspNetCore.Http;

namespace PosSystem.Api.Security
{
    public class RequestBodyBufferingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestBodyBufferingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Enable buffering to allow multiple reads of the request body
            if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
            {
                context.Request.EnableBuffering();
            }

            await _next(context);
        }
    }
}