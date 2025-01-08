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
        public async Task<ActionResult<ApiResponse<string>>> ScreenLogin([FromBody] LoginScreenDto loginDto)
        {
            try
            {
                _logger.LogInformation($"Login attempt with MAC: {loginDto.MACAddress}");
                
                var screenPassword = _configuration["ScreenSettings:Password"];
                _logger.LogInformation($"Configured password: {screenPassword}");

                if (loginDto.Password != screenPassword)
                {
                    _logger.LogWarning($"Invalid password for MAC: {loginDto.MACAddress}");
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid credentials"
                    });
                }

                var screen = await _context.Screens
                    .FirstOrDefaultAsync(s => s.MACAddress == loginDto.MACAddress);

                if (screen == null)
                {
                    _logger.LogWarning($"No screen found with MAC: {loginDto.MACAddress}");
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Screen not found"
                    });
                }

                _logger.LogInformation($"Screen found: {screen.Name}");

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
                _logger.LogError($"Error in screen login: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Error processing login request"
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

        [HttpPost("track-displays")]
        [Authorize(Roles = "Screen")]
        public async Task<ActionResult<ApiResponse<bool>>> TrackDisplays([FromBody] List<DisplayTracker> displays)
        {
            try
            {
                if (displays == null || !displays.Any())
                {
                    _logger.LogWarning("Received empty or null display tracking data");
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "No tracking data provided"
                    });
                }

                var screenId = int.Parse(User.FindFirst("ScreenId")?.Value);
                _logger.LogInformation($"Received tracking request for screen {screenId} with {displays.Count} items");
                
                // Log the incoming data
                foreach(var display in displays)
                {
                    _logger.LogInformation($"Incoming track: Type={display.ItemType}, ID={display.ItemId}");
                    display.ScreenId = screenId;
                    display.DisplayedAt = DateTime.UtcNow;
                    _context.DisplayTrackers.Add(display);
                }
                
                var saveResult = await _context.SaveChangesAsync();
                _logger.LogInformation($"Saved {saveResult} records to database");
                
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = true,
                    Message = $"Display tracking recorded successfully: {saveResult} records"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error tracking displays: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = $"Error recording display tracking: {ex.Message}"
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