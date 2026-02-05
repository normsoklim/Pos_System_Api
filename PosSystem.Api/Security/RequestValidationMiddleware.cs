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
             // Check if the request is a multipart request (file upload)
             // This handles cases where content type might be "multipart/form-data; boundary=..." or similar
             var requestContentType = context.Request.ContentType;
             bool isMultipartRequest = !string.IsNullOrEmpty(requestContentType) &&
                                      requestContentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);
             
             // Skip all validation for multipart requests (file uploads) as they contain special characters that may trigger false positives
             if (isMultipartRequest)
             {
                 await _next(context);
                 return;
             }
             
             // Validate request headers for potential security threats
             // But skip header validation for multipart requests as well to be extra safe
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

              // For POST/PUT/PATCH requests, validate the body content
              if (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH")
              {
                  // Double-check content type for multipart requests
                  var contentType = context.Request.ContentType;
                  bool isMultipartBody = !string.IsNullOrEmpty(contentType) &&
                                        contentType.StartsWith("multipart/", StringComparison.OrdinalIgnoreCase);
                  
                  // Skip validation for multipart requests (file uploads) as they contain special characters that may trigger false positives
                  if (isMultipartBody)
                  {
                      await _next(context);
                      return;
                  }
                  
                  var bodyText = string.Empty;
                  
                  try
                  {
                      // Only attempt to read the body if we can and should
                      if (context.Request.ContentLength > 0 && context.Request.Body.CanRead && !isMultipartBody)
                      {
                          // Reset the stream position to the beginning before reading
                          if (context.Request.Body.CanSeek)
                          {
                              context.Request.Body.Position = 0;
                          }
                          
                          using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                          bodyText = await reader.ReadToEndAsync();
                          
                          // Reset the body stream position for the next middleware/controller
                          if (context.Request.Body.CanSeek)
                          {
                              context.Request.Body.Position = 0;
                          }
                      }
                  }
                  catch (Exception)
                  {
                      // If we can't read the body for any reason, skip validation
                      // This can happen with multipart requests where the body is not readable as text
                      await _next(context);
                      return;
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

            // Check for potential SQL injection patterns - more specific patterns to avoid false positives
            var sqlInjectionPatterns = new[]
            {
                @"\b(union\s+select|insert\s+into|update\s+\w+\s+set|delete\s+from|drop\s+(table|database)|create\s+(table|database)|alter\s+table|exec\s*\(|execute\s*\(|sp_|xp_|sysobjects|syscolumns)\b",
                @"';\s*(--|\/\*|exec|execute|drop|create|alter|declare|open|close|fetch|kill)",
                @"\b(and|or)\b\s*\d+\s*=\s*\d+\s*(and|or)\s*\d+\s*=\s*\d+" // Boolean-based SQLi
            };

            foreach (var pattern in sqlInjectionPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                {
                    return true;
                }
            }

            // Check for potential XSS patterns - more specific to avoid false positives
            var xssPatterns = new[]
            {
                @"<script[^>]*>.*?</script>",
                @"<img[^>]*\s+src\s*=\s*['""]\s*javascript:",
                @"<a[^>]*\s+href\s*=\s*['""]\s*javascript:",
                @"on\w+\s*=\s*['""][^'""]*script[^'""]*['""]",
                @"<iframe[^>]*>.*?</iframe>",
                @"<object[^>]*>.*?</object>",
                @"<embed[^>]*\s+src\s*=",
                @"eval\s*\(\s*.*\s*\)",
                @"expression\s*\(\s*.*\s*\)"
            };

            foreach (var pattern in xssPatterns)
            {
                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                {
                    return true;
                }
            }

            // Check for potential path traversal
            if (Regex.IsMatch(content, @"\.\.\\|\.\\|\.\./|\.\/", RegexOptions.IgnoreCase))
            {
                // Additional check to see if it's part of a legitimate file path or URL
                if (!Regex.IsMatch(content, @"^[\w\/\-.]+[\/\\][\w\/\-.]+\.(jpg|jpeg|png|gif|webp|bmp|svg|css|js|pdf|txt|html|htm)$", RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}