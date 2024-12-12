using ClassLibrary.DtoModels.Common;
using System.Text.Json;
using ClassLibrary.DtoModels.Common;
using WebApplication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace TheWebApplication.Security
{
    public class CustomJwtBearerEvents : JwtBearerEvents
    {
        public override Task AuthenticationFailed(AuthenticationFailedContext context)
        {
            context.NoResult();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Authentication failed",
                Errors = new List<string> { "Invalid token or token expired" }
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        public override Task Challenge(JwtBearerChallengeContext context)
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Authentication failed",
                Errors = new List<string> { "You are not authenticated. Please login." }
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        public override Task Forbidden(ForbiddenContext context)
        {
            context.Response.StatusCode = 403;
            context.Response.ContentType = "application/json";

            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Authorization failed",
                Errors = new List<string> { "You don't have permission to access this resource." }
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
} 