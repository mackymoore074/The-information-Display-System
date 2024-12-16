using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SystemModels.API;
using SystemModels.Models;
using SystemModels;
using SystemModels.DtoModels.Authorization;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.DtoModels.Auth;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.Models;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly InfoDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<Admin> _passwordHasher;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            InfoDbContext context,
            IConfiguration configuration,
            IPasswordHasher<Admin> passwordHasher,
            ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Hardcoded Admin
                var hardcodedAdmin = new Admin
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("adminpassword123"),  // Store the password hash, not plaintext
                    Email = "admin@company.com",
                    DateCreated = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    Role = Role.Admin,
                };

                // Check if the login matches the hardcoded admin
                if (loginDto.Email == hardcodedAdmin.Email && BCrypt.Net.BCrypt.Verify(loginDto.Password, hardcodedAdmin.PasswordHash))
                {
                    var token = await GenerateJwtToken(hardcodedAdmin);

                    return Ok(new ApiResponse<AuthResponseDto>
                    {
                        Success = true,
                        Message = "Authentication successful",
                        Data = new AuthResponseDto
                        {
                            Token = token,
                            Role = hardcodedAdmin.Role.ToString(),
                            AdminId = hardcodedAdmin.Id,
                            Email = hardcodedAdmin.Email,
                            FirstName = hardcodedAdmin.FirstName,
                            LastName = hardcodedAdmin.LastName
                        }
                    });
                }

                // Check database for the admin
                var admin = await _context.Admins /**/
                    .Include(a => a.Agency)
                    .FirstOrDefaultAsync(a => a.Email == loginDto.Email);

                if (admin == null)
                {
                    return Unauthorized(new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Authentication failed",
                        Errors = new List<string> { "Invalid email or password" }
                    });
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.PasswordHash))
                {
                    return Unauthorized(new ApiResponse<AuthResponseDto>
                    {
                        Success = false,
                        Message = "Authentication failed",
                        Errors = new List<string> { "Invalid email or password" }
                    });
                }

                var tokenForDbUser = await GenerateJwtToken(admin);

                admin.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<AuthResponseDto>
                {
                    Success = true,
                    Message = "Authentication successful",
                    Data = new AuthResponseDto
                    {
                        Token = tokenForDbUser,
                        Role = admin.Role.ToString(),
                        AdminId = admin.Id,
                        Email = admin.Email,
                        FirstName = admin.FirstName,
                        LastName = admin.LastName
                    }
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in Login", ex);
                return StatusCode(500, new ApiResponse<AuthResponseDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        private async Task<string> GenerateJwtToken(Admin admin)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:Key"])); // Uses a secret key from the app's configuration to secure the token
            // and This ensures the token is signed and validated securely.

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature); // this Specifies the algorithm (HmacSha512) used to sign the token.

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim(ClaimTypes.Role, admin.Role.ToString()),
                new Claim("AgencyId", admin.AgencyId?.ToString() ?? "")
            };

            var token = new JwtSecurityToken(                    // Specifies the token's
                issuer: _configuration["JwtSettings:Issuer"], // Who created the token (e.g., our app or API).
                audience: _configuration["JwtSettings:Audience"], // Who can use the token (e.g., the client app).
                claims: claims, // The user's information included in the token.
                expires: DateTime.Now.AddDays(1), // Sets the token's validity period (1 day in this case).
                signingCredentials: creds); // Ensures the token is signed and secure.

            return new JwtSecurityTokenHandler().WriteToken(token);
            /* This Converts the JwtSecurityToken into a compact string format for transmission to the client 
             * The Purpose
            Generates a secure token for the client.
            The client uses this token in subsequent API requests as proof of authentication.
            The server validates the token to ensure the request is from a legitimate user.
             */

        }

        private async Task LogErrorToDatabaseAsync(string context, Exception ex)
        {
            _logger.LogError($"{context}: {ex.Message}", ex); // Logs the error message and exception stack trace using the application's logging system.

            try
            {
                var errorLog = new ErrorLog // Creates an ErrorLog object with the exception details.
                {
                    ErrorMessage = ex.Message,
                    DateCreated = DateTime.UtcNow
                };

                _context.ErrorLogs.Add(errorLog);
                await _context.SaveChangesAsync(); // Saves the error to the database for long-term storage and reporting.
            }
            catch (Exception logEx)
            {
                _logger.LogError($"Failed to log error to database: {logEx.Message}", logEx); /*
                       If logging to the database fails, logs the failure to the application's logging system.
                                Purpose:
                                Tracks errors for debugging and resolving issues in the application.
                                Centralized error storage in the database allows:
                                Easy querying and reporting of error trends.
                                Better understanding of frequent or critical issues.
                                                                                               */
            }
            /* 
             LogErrorToDatabaseAsync:
                Troubleshooting: Helps developers and admins identify and resolve issues efficiently.
                Error Tracking: Provides historical data about application errors.
                Audit Trail: Logs serve as a record for investigating security incidents or performance problems.
             */
        }
    }
}
