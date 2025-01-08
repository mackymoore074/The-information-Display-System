using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using System.Text.Encodings.Web;
using ClassLibrary.DtoModels.Screen;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScreenController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<ScreenController> _logger;
        private readonly IConfiguration _configuration;

        public ScreenController(ClassDBContext context, ILogger<ScreenController> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        private int GetCurrentAdminId()
        {
            var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == "AdminId");
            if (adminIdClaim == null)
            {
                throw new UnauthorizedAccessException("Admin ID not found in token");
            }
            return int.Parse(adminIdClaim.Value);
        }

        // GET: api/screen
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ScreenDto>>>> GetScreens()
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();

                var screens = await _context.Screens
                    .Include(s => s.Location)
                    .Include(s => s.Department)
                    .Include(s => s.Agency)
                    .Where(s => s.AdminId == currentAdminId)
                    .Select(s => new ScreenDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        LocationId = s.LocationId,
                        AgencyId = s.AgencyId,
                        DepartmentId = s.DepartmentId,
                        AdminId = s.AdminId,
                        ScreenType = s.ScreenType ?? string.Empty,
                        IsOnline = s.IsOnline,
                        StatusMessage = s.StatusMessage ?? string.Empty,
                        MACAddress = s.MACAddress ?? string.Empty,
                        LastUpdated = s.LastUpdated,
                        LastCheckedIn = s.LastCheckedIn,
                        LocationName = s.Location.Name ?? string.Empty,
                        DepartmentName = s.Department.Name ?? string.Empty,
                        AgencyName = s.Agency.Name ?? string.Empty
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<ScreenDto>>
                {
                    Success = true,
                    Message = screens.Any() ? "Screens retrieved successfully" : "No screens found",
                    Data = screens
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetScreens", ex);
                return StatusCode(500, new ApiResponse<List<ScreenDto>>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = new List<ScreenDto>()
                });
            }
        }

        // GET: api/screen/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ScreenDto>>> GetScreen(int id)
        {
            try
            {
                var screen = await _context.Screens
                    .Include(s => s.Location)
                    .Include(s => s.Department)
                    .Include(s => s.Agency)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (screen == null)
                    return NotFound(new ApiResponse<ScreenDto>
                    {
                        Success = false,
                        Message = $"Screen with ID {id} not found",
                        Data = default
                    });

                var screenDto = new ScreenDto
                {
                    Id = screen.Id,
                    Name = screen.Name,
                    LocationId = screen.LocationId,
                    AgencyId = screen.AgencyId,
                    DepartmentId = screen.DepartmentId,
                    AdminId = screen.AdminId,
                    ScreenType = screen.ScreenType ?? string.Empty,
                    IsOnline = screen.IsOnline,
                    StatusMessage = screen.StatusMessage ?? string.Empty,
                    MACAddress = screen.MACAddress ?? string.Empty,
                    LastUpdated = screen.LastUpdated,
                    LastCheckedIn = screen.LastCheckedIn,
                    LocationName = screen.Location?.Name ?? string.Empty,
                    DepartmentName = screen.Department?.Name ?? string.Empty,
                    AgencyName = screen.Agency?.Name ?? string.Empty
                };

                return Ok(new ApiResponse<ScreenDto>
                {
                    Success = true,
                    Message = "Screen retrieved successfully",
                    Data = screenDto
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetScreen", ex);
                var response = new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = default
                };
                response.Errors.Add(ex.Message);
                return StatusCode(500, response);
            }
        }

        // POST: api/screen
        [HttpPost]
        public async Task<ActionResult<ApiResponse<ScreenDto>>> CreateScreen([FromBody] CreateScreenDto createScreenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList()
                });
            }

            try
            {
                int currentAdminId = GetCurrentAdminId();

                // Validate Location
                var location = await _context.Locations
                    .FirstOrDefaultAsync(l => l.Id == int.Parse(createScreenDto.LocationId) && 
                                            l.AdminId == currentAdminId);
                if (location == null)
                {
                    return BadRequest(new ApiResponse<ScreenDto>
                    {
                        Success = false,
                        Message = "Invalid Location ID or you don't have permission to use this location"
                    });
                }

                // Validate Agency
                var agency = await _context.Agencies
                    .FirstOrDefaultAsync(a => a.Id == int.Parse(createScreenDto.AgencyId) && 
                                            a.AdminId == currentAdminId);
                if (agency == null)
                {
                    return BadRequest(new ApiResponse<ScreenDto>
                    {
                        Success = false,
                        Message = "Invalid Agency ID or you don't have permission to use this agency"
                    });
                }

                // Validate Department if provided
                if (!string.IsNullOrEmpty(createScreenDto.DepartmentId))
                {
                    var department = await _context.Departments
                        .FirstOrDefaultAsync(d => d.Id == int.Parse(createScreenDto.DepartmentId) && 
                                                (d.Location.AdminId == currentAdminId || 
                                                 d.Agency.AdminId == currentAdminId));
                    if (department == null)
                    {
                        return BadRequest(new ApiResponse<ScreenDto>
                        {
                            Success = false,
                            Message = "Invalid Department ID or you don't have permission to use this department"
                        });
                    }
                }

                var screen = new Screen
                {
                    Name = createScreenDto.Name,
                    Description = createScreenDto.Description,
                    LocationId = int.Parse(createScreenDto.LocationId),
                    AgencyId = int.Parse(createScreenDto.AgencyId),
                    DepartmentId = !string.IsNullOrEmpty(createScreenDto.DepartmentId) ? 
                        int.Parse(createScreenDto.DepartmentId) : null,
                    AdminId = currentAdminId,
                    ScreenType = createScreenDto.ScreenType,
                    IsOnline = createScreenDto.IsOnline,
                    StatusMessage = createScreenDto.StatusMessage,
                    MACAddress = createScreenDto.MACAddress,
                    LastUpdated = DateTime.UtcNow,
                    LastCheckedIn = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow
                };

                _context.Screens.Add(screen);
                await _context.SaveChangesAsync();

                // Load related entities for the response
                await _context.Entry(screen)
                    .Reference(s => s.Location)
                    .LoadAsync();
                await _context.Entry(screen)
                    .Reference(s => s.Department)
                    .LoadAsync();
                await _context.Entry(screen)
                    .Reference(s => s.Agency)
                    .LoadAsync();

                var screenDto = new ScreenDto
                {
                    Id = screen.Id,
                    Name = screen.Name,
                    LocationId = screen.LocationId,
                    AgencyId = screen.AgencyId,
                    DepartmentId = screen.DepartmentId,
                    AdminId = screen.AdminId,
                    ScreenType = screen.ScreenType ?? string.Empty,
                    IsOnline = screen.IsOnline,
                    StatusMessage = screen.StatusMessage ?? string.Empty,
                    MACAddress = screen.MACAddress ?? string.Empty,
                    LastUpdated = screen.LastUpdated,
                    LastCheckedIn = screen.LastCheckedIn,
                    LocationName = screen.Location?.Name ?? string.Empty,
                    DepartmentName = screen.Department?.Name ?? string.Empty,
                    AgencyName = screen.Agency?.Name ?? string.Empty
                };

                return CreatedAtAction(
                    nameof(GetScreen), 
                    new { id = screen.Id }, 
                    new ApiResponse<ScreenDto>
                    {
                        Success = true,
                        Message = "Screen created successfully",
                        Data = screenDto
                    });
            }
            catch (FormatException ex)
            {
                _logger.LogError($"Error parsing screen data: {ex.Message}");
                return BadRequest(new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Invalid ID format provided",
                    Errors = new List<string> { ex.Message }
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"Database error while saving screen: {dbEx.Message}");
                if (dbEx.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {dbEx.InnerException.Message}");
                }
                return StatusCode(500, new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Database error occurred",
                    Errors = new List<string> { dbEx.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error creating screen: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        // PUT: api/screen/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ScreenDto>>> UpdateScreen(int id, [FromBody] UpdateScreenDto updateScreenDto)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();

                var screen = await _context.Screens
                    .FirstOrDefaultAsync(s => s.Id == id && s.AdminId == currentAdminId);

                if (screen == null)
                {
                    return NotFound(new ApiResponse<ScreenDto>
                    {
                        Success = false,
                        Message = "Screen not found or you don't have permission to update it"
                    });
                }

                // Validate Location, Agency, and Department ownership similar to Create method
                // ... add validation logic here ...

                // Update screen properties
                screen.Name = updateScreenDto.Name;
                screen.LocationId = int.Parse(updateScreenDto.LocationId);
                screen.AgencyId = int.Parse(updateScreenDto.AgencyId);
                screen.DepartmentId = !string.IsNullOrEmpty(updateScreenDto.DepartmentId) ? 
                    int.Parse(updateScreenDto.DepartmentId) : null;
                screen.ScreenType = updateScreenDto.ScreenType;
                screen.LastUpdated = DateTime.UtcNow;
                screen.IsOnline = updateScreenDto.IsOnline;
                screen.StatusMessage = updateScreenDto.StatusMessage;
                screen.MACAddress = updateScreenDto.MACAddress;

                await _context.SaveChangesAsync();

                var screenDto = new ScreenDto
                {
                    Id = screen.Id,
                    Name = screen.Name,
                    LocationId = screen.LocationId,
                    AgencyId = screen.AgencyId,
                    DepartmentId = screen.DepartmentId,
                    AdminId = screen.AdminId,
                    ScreenType = screen.ScreenType ?? string.Empty,
                    IsOnline = screen.IsOnline,
                    StatusMessage = screen.StatusMessage ?? string.Empty,
                    MACAddress = screen.MACAddress ?? string.Empty,
                    LastUpdated = screen.LastUpdated,
                    LastCheckedIn = screen.LastCheckedIn,
                    LocationName = screen.Location?.Name ?? string.Empty,
                    DepartmentName = screen.Department?.Name ?? string.Empty,
                    AgencyName = screen.Agency?.Name ?? string.Empty
                };

                return Ok(new ApiResponse<ScreenDto>
                {
                    Success = true,
                    Message = "Screen updated successfully",
                    Data = screenDto
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in UpdateScreen", ex);
                return StatusCode(500, new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // DELETE: api/screen/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteScreen(int id)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();

                var screen = await _context.Screens
                    .FirstOrDefaultAsync(s => s.Id == id && s.AdminId == currentAdminId);

                if (screen == null)
                {
                    return NotFound(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Screen not found or you don't have permission to delete it"
                    });
                }

                _context.Screens.Remove(screen);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Screen deleted successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteScreen", ex);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        // Helper method to log errors to the database
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

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<string>>> Login([FromBody] LoginScreenDto loginDto)
        {
            _logger.LogInformation("Login attempt for screen: {ScreenName}", loginDto.ScreenName);

            try
            {
                var screen = await _context.Screens
                    .FirstOrDefaultAsync(s => 
                        s.Name.ToLower() == loginDto.ScreenName.ToLower() && 
                        s.MACAddress.ToLower() == loginDto.MacAddress.ToLower());

                if (screen == null)
                {
                    _logger.LogWarning("Invalid login attempt for screen: {ScreenName}", loginDto.ScreenName);
                    return Unauthorized(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Invalid screen credentials"
                    });
                }

                _logger.LogInformation("Successful login for screen: {ScreenName}", loginDto.ScreenName);
                var token = GenerateJwtToken(screen);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Login successful",
                    Data = token
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in screen login");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Error during login",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [Authorize]
        [HttpGet("news-items")]
        public async Task<ActionResult<ApiResponse<List<NewsItem>>>> GetNewsItems()
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var newsItems = await _context.NewsItems
                    .Where(n => n.TimeOutDate > currentTime)
                    .OrderByDescending(n => n.DateCreated)
                    .ToListAsync();

                return Ok(new ApiResponse<List<NewsItem>>
                {
                    Success = true,
                    Data = newsItems
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error getting news items", ex);
                return StatusCode(500, new ApiResponse<List<NewsItem>>
                {
                    Success = false,
                    Message = "Error retrieving news items",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [Authorize]
        [HttpGet("menu-items")]
        public async Task<ActionResult<ApiResponse<List<MenuItem>>>> GetMenuItems()
        {
            try
            {
                var currentTime = DateTime.UtcNow;
                var menuItems = await _context.MenuItems
                    .Where(m => m.TimeOutDate > currentTime)
                    .OrderByDescending(m => m.DateCreated)
                    .ToListAsync();

                return Ok(new ApiResponse<List<MenuItem>>
                {
                    Success = true,
                    Data = menuItems
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error getting menu items", ex);
                return StatusCode(500, new ApiResponse<List<MenuItem>>
                {
                    Success = false,
                    Message = "Error retrieving menu items",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        private string GenerateJwtToken(Screen screen)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, screen.Name),
                new Claim("ScreenId", screen.Id.ToString()),
                new Claim("MacAddress", screen.MACAddress)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["JwtSettings:ExpirationInDays"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
