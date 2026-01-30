using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.RegularExpressions;

namespace PosSystem.Api.Filters
{
    public class InputValidationFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Validate all input parameters for potential security threats
            foreach (var parameter in context.ActionArguments)
            {
                if (parameter.Value != null)
                {
                    var value = parameter.Value.ToString();
                    if (!string.IsNullOrEmpty(value) && HasSecurityThreats(value))
                    {
                        context.Result = new BadRequestObjectResult(new
                        {
                            error = "Input validation failed",
                            message = "Input contains potential security threats"
                        });
                        return;
                    }
                }
            }

            // Validate route values
            foreach (var routeValue in context.RouteData.Values)
            {
                if (routeValue.Value != null)
                {
                    var value = routeValue.Value.ToString();
                    if (!string.IsNullOrEmpty(value) && HasSecurityThreats(value))
                    {
                        context.Result = new BadRequestObjectResult(new
                        {
                            error = "Input validation failed",
                            message = "Route parameter contains potential security threats"
                        });
                        return;
                    }
                }
            }

            // Validate query string parameters
            foreach (var queryParam in context.HttpContext.Request.Query)
            {
                var value = queryParam.Value.ToString();
                if (!string.IsNullOrEmpty(value) && HasSecurityThreats(value))
                {
                    context.Result = new BadRequestObjectResult(new
                    {
                        error = "Input validation failed",
                        message = "Query parameter contains potential security threats"
                    });
                    return;
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // No action needed after execution
        }

        private bool HasSecurityThreats(string content)
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