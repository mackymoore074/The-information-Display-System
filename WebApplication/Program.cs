using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using TheWebApplication.Middleware;
using TheWebApplication.Security;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using ClassLibrary;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

// Add services to the container.

// Add services to the container
builder.Services.AddScoped<IPasswordHasher<Admin>, PasswordHasher<Admin>>();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add DbContext for EF Core
builder.Services.AddDbContext<ClassDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ClassDB")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Define the Bearer Authentication scheme
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter the JWT Bearer token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Apply security globally to all endpoints
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
// Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"]
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            context.NoResult();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Authentication failed",
                Data = null,
                Errors = new List<string> { context.Exception.Message }
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        },
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = "Unauthorized access",
                Data = null,
                Errors = new List<string> { "You are not authorized to access this resource" }
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    };
});

builder.Services.AddScoped<CustomJwtBearerEvents>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
        policy.WithOrigins("https://localhost:7218", "http://localhost:5059") // Use separate strings for each origin
              .AllowAnyHeader()
              .AllowAnyMethod());
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Enable detailed exception page in development
    app.UseSwagger(); // Serve the Swagger JSON
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    });
}

// Use HTTPS and enable CORS
app.UseHttpsRedirection();
app.UseCors("AllowBlazor");

// Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Custom middleware should come after authentication
app.UseMiddleware<CustomAuthorizationMiddleware>();

// Map controllers and apply CORS policy
app.MapControllers().RequireCors("AllowBlazor");

// Add this debugging middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"Request Path: {context.Request.Path}");
    Console.WriteLine($"Request Method: {context.Request.Method}");
    Console.WriteLine($"Authorization Header: {context.Request.Headers["Authorization"]}");
    
    var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
    if (token != null)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            Console.WriteLine("Token Details:");
            Console.WriteLine($"Issuer: {jsonToken?.Issuer}");
            Console.WriteLine($"Valid From: {jsonToken?.ValidFrom}");
            Console.WriteLine($"Valid To: {jsonToken?.ValidTo}");
            Console.WriteLine("Claims:");
            if (jsonToken?.Claims != null)
            {
                foreach (var claim in jsonToken.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Token parsing error: {ex.GetType().Name} - {ex.Message}");
        }
    }
    else
    {
        Console.WriteLine("No token found in request");
    }

    await next();
});

// Run the application
app.Run();



