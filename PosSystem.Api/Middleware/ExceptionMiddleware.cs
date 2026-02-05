using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PosSystem.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log the full exception details for internal use
                _logger.LogError(ex, "An unhandled exception occurred. Request: {Method} {Path}", 
                    context.Request?.Method, context.Request?.Path);

                // Create a user-friendly error response without exposing sensitive information
                await HandleExceptionAsync(context, ex);
            }
        }

         private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
         {
             // Check if the response has already started (headers sent), if so, we can't modify it
             if (context.Response.HasStarted)
             {
                 // Log the error but can't send response since headers are already sent
                 return;
             }

             context.Response.ContentType = "application/json";

             var response = new
             {
                 Error = "An error occurred while processing your request",
                 Message = "Internal Server Error",
                 RequestId = context.TraceIdentifier
             };

             // Set appropriate status code based on exception type
             context.Response.StatusCode = exception switch
             {
                 UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                 ArgumentException => StatusCodes.Status400BadRequest,
                 KeyNotFoundException => StatusCodes.Status404NotFound,
                 InvalidOperationException => StatusCodes.Status409Conflict,
                 _ => StatusCodes.Status500InternalServerError
             };

             // Serialize and write the response
             var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
             {
                 PropertyNamingPolicy = JsonNamingPolicy.CamelCase
             });

             await context.Response.WriteAsync(jsonResponse);
         }
    }
}