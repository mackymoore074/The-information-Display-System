using ClassLibrary.API;
using System.Net;
using System.Text.Json;

namespace TheWebApplication.Middleware
{
    public class CustomAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.User.Identity.IsAuthenticated && 
                !context.Request.Path.StartsWithSegments("/api/auth") &&
                !context.Request.Path.StartsWithSegments("/api/screen/by-ip") &&
                !context.Request.Path.Value.Contains("/api/screen/", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await HandleUnauthorizedResponse(context);
                return;
            }

            await _next(context);
        }

        private async Task HandleUnauthorizedResponse(HttpContext context)
        {
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Authentication failed",
                Errors = new List<string> { "You are not authenticated. Please login." }
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private async Task HandleForbiddenResponse(HttpContext context)
        {
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Authorization failed",
                Errors = new List<string> { "You don't have permission to access this resource." }
            };

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
} 