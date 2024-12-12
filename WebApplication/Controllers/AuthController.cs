using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClassLibrary;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Admin;
using ClassLibrary.DtoModels.Auth;
using ClassLibrary.DtoModels.Common;

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
                var admin = await _context.Admins
                    .Include(a => a.Agency)
                    .FirstOrDefaultAsync(a => a.Email == loginDto.Email);

                if (admin == null)
                    return Unauthorized(new ApiResponse<AuthResponseDto> 
                    { 
                        Success = false,
                        Message = "Authentication failed",
                        Errors = new List<string> { "Invalid email or password" }
                    });

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.PasswordHash))
                    return Unauthorized(new ApiResponse<AuthResponseDto> 
                    { 
                        Success = false,
                        Message = "Authentication failed",
                        Errors = new List<string> { "Invalid email or password" }
                    });

                var token = GenerateJwtToken(admin);
                
                admin.LastLogin = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<AuthResponseDto> 
                { 
                    Success = true,
                    Message = "Authentication successful",
                    Data = new AuthResponseDto 
                    {
                        Token = token,
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

        private string GenerateJwtToken(Admin admin)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new Claim(ClaimTypes.Email, admin.Email),
                new Claim(ClaimTypes.Role, admin.Role.ToString()),
                new Claim("AgencyId", admin.AgencyId?.ToString() ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["JwtSettings:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task LogErrorToDatabaseAsync(string context, Exception ex)
        {
            _logger.LogError($"{context}: {ex.Message}", ex);

            try
            {
                var errorLog = new ErrorLog
                {
                    ErrorMessage = ex.Message,
                    DateCreated = DateTime.UtcNow
                };

                _context.ErrorLogs.Add(errorLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError($"Failed to log error to database: {logEx.Message}", logEx);
            }
        }
    }

}
