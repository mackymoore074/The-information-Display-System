using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Authorization;
using ClassLibrary.DtoModels.Screen;
using ClassLibrary.API;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<Admin> _passwordHasher;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            ClassDBContext context,
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
                    var token = GenerateJwtToken(hardcodedAdmin);

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

                var tokenForDbUser = GenerateJwtToken(admin);

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

        [HttpPost("screen/login")]
        public async Task<ActionResult<ApiResponse<string>>> ScreenLogin([FromBody] LoginScreenDto loginDto)
        {
            try
            {
                _logger.LogInformation($"Screen login attempt - Name: {loginDto.ScreenName}");

                var screen = await _context.Screens
                    .FirstOrDefaultAsync(s => 
                        s.Name.ToLower() == loginDto.ScreenName.ToLower() && 
                        s.MACAddress.ToLower() == loginDto.MacAddress.ToLower());

                if (screen == null)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid screen credentials"
                    });
                }

                // Generate token with Screen role and ScreenId
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, screen.Name),
                    new Claim("ScreenId", screen.Id.ToString()),
                    new Claim(ClaimTypes.Role, "Screen")  // Make sure this role claim is present
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["JwtSettings:Issuer"],
                    audience: _configuration["JwtSettings:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtSettings:ExpirationInDays"])),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = tokenString,
                    Message = "Screen authenticated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Screen login error: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Error during screen authentication"
                });
            }
        }

        private string GenerateJwtToken(Admin admin)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim("AdminId", admin.Id.ToString()),
                new Claim(ClaimTypes.Role, admin.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateJwtToken(Screen screen)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, screen.Name),
                new Claim("ScreenId", screen.Id.ToString()),
                new Claim("MacAddress", screen.MACAddress),
                new Claim(ClaimTypes.Role, "Screen")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtSettings:ExpirationInDays"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
