using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Screen;
using ClassLibrary.DtoModels.Common;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/screenauth")]
    public class ScreenAuthController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ScreenAuthController> _logger;

        public ScreenAuthController(
            ClassDBContext context,
            IConfiguration configuration,
            ILogger<ScreenAuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<string>>> Login([FromBody] LoginScreenDto loginDto)
        {
            try
            {
                _logger.LogInformation($"Screen login attempt - Name: {loginDto.ScreenName}, MAC: {loginDto.MacAddress}");

                var screen = await _context.Screens
                    .FirstOrDefaultAsync(s => 
                        s.Name.ToLower() == loginDto.ScreenName.ToLower() && 
                        s.MACAddress.ToLower() == loginDto.MacAddress.ToLower());

                if (screen == null)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Authentication failed",
                        Errors = new List<string> { "Invalid screen credentials" }
                    });
                }

                var token = GenerateJwtToken(screen);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Authentication successful",
                    Data = token
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Login error", ex);
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        [HttpGet("menu-items")]
        [Authorize(Roles = "Screen")]
        public async Task<ActionResult<ApiResponse<List<MenuItem>>>> GetMenuItems()
        {
            try
            {
                _logger.LogInformation("Getting menu items");
                
                var items = await _context.MenuItems
                    .AsNoTracking()
                    .Where(m => m.IsActive)
                    .Select(m => new MenuItem 
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Description = m.Description,
                        Price = m.Price,
                        IsActive = m.IsActive,
                        DateCreated = m.DateCreated,
                        AdminId = m.AdminId
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {items.Count} menu items");

                return Ok(new ApiResponse<List<MenuItem>>
                {
                    Success = true,
                    Data = items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetMenuItems: {ex.Message}");
                return StatusCode(500, new ApiResponse<List<MenuItem>>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        [HttpGet("news-items")]
        [Authorize(Roles = "Screen")]
        public async Task<ActionResult<ApiResponse<List<NewsItem>>>> GetNewsItems()
        {
            try
            {
                _logger.LogInformation("Getting news items");
                
                var items = await _context.NewsItems
                    .AsNoTracking()
                    .Where(n => n.IsActive)
                    .Select(n => new NewsItem 
                    {
                        Id = n.Id,
                        Title = n.Title,
                        NewsItemBody = n.NewsItemBody,
                        IsActive = n.IsActive,
                        DateCreated = n.DateCreated,
                        AdminId = n.AdminId
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {items.Count} news items");

                return Ok(new ApiResponse<List<NewsItem>>
                {
                    Success = true,
                    Data = items
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetNewsItems: {ex.Message}");
                return StatusCode(500, new ApiResponse<List<NewsItem>>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        private string GenerateJwtToken(Screen screen)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, screen.Name),
                new Claim("ScreenId", screen.Id.ToString()),
                new Claim(ClaimTypes.Role, "Screen")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["JwtSettings:ExpirationInDays"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task LogErrorToDatabaseAsync(string context, Exception ex)
        {
            _logger.LogError($"{context}: {ex.Message}");

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
                _logger.LogError($"Failed to log error to database: {logEx.Message}");
            }
        }
    }
} 