using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using System.Text.Encodings.Web;
using ClassLibrary.DtoModels.Screen;
using System.Globalization;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScreenController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<ScreenController> _logger;

        public ScreenController(ClassDBContext context, ILogger<ScreenController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/screen
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<ScreenDto>>>> GetScreens()
        {
            try
            {
                var screens = await _context.Screens
                    .Include(s => s.Location)
                    .Include(s => s.Department)
                    .Include(s => s.Agency)
                    .Select(s => new ScreenDto
                    {
                        Id = s.Id,
                        Name = s.Name,
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
                    Message = "Screens retrieved successfully",
                    Data = screens
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetScreens", ex);
                var response = new ApiResponse<List<ScreenDto>>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = new List<ScreenDto>()
                };
                response.Errors.Add(ex.Message);
                return StatusCode(500, response);
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
                var response = new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Invalid model state"
                };
                response.Errors.AddRange(ModelState.Values
                    .SelectMany(x => x.Errors)
                    .Select(x => x.ErrorMessage));
                return BadRequest(response);
            }

            try
            {
                _logger.LogInformation($"Creating screen with Name: {createScreenDto.Name}, " +
                    $"LocationId: {createScreenDto.LocationId}, " +
                    $"AgencyId: {createScreenDto.AgencyId}, " +
                    $"DepartmentId: {createScreenDto.DepartmentId}, " +
                    $"AdminId: {createScreenDto.AdminId}");

                var screen = new Screen
                {
                    Name = createScreenDto.Name,
                    Description = createScreenDto.Description,
                    LocationId = string.IsNullOrEmpty(createScreenDto.LocationId) ? 0 : int.Parse(createScreenDto.LocationId),
                    AgencyId = string.IsNullOrEmpty(createScreenDto.AgencyId) ? 0 : int.Parse(createScreenDto.AgencyId),
                    DepartmentId = string.IsNullOrEmpty(createScreenDto.DepartmentId) ? null : int.Parse(createScreenDto.DepartmentId),
                    AdminId = 1,
                    ScreenType = createScreenDto.ScreenType,
                    IsOnline = createScreenDto.IsOnline,
                    StatusMessage = createScreenDto.StatusMessage,
                    MACAddress = createScreenDto.MACAddress,
                    LastUpdated = DateTime.UtcNow,
                    LastCheckedIn = DateTime.UtcNow,
                    DateCreated = DateTime.UtcNow
                };

                _logger.LogInformation($"Parsed screen data: " +
                    $"LocationId: {screen.LocationId}, " +
                    $"AgencyId: {screen.AgencyId}, " +
                    $"DepartmentId: {screen.DepartmentId}, " +
                    $"AdminId: {screen.AdminId}");

                _context.Screens.Add(screen);

                try 
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError($"Database error: {dbEx.Message}");
                    if (dbEx.InnerException != null)
                    {
                        _logger.LogError($"Inner exception: {dbEx.InnerException.Message}");
                    }
                    throw;
                }

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
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Invalid model state",
                    Data = default
                });

            try
            {
                var screen = await _context.Screens.FindAsync(id);
                if (screen == null)
                    return NotFound(new ApiResponse<ScreenDto>
                    {
                        Success = false,
                        Message = $"Screen with ID {id} not found",
                        Data = default
                    });

                if (!int.TryParse(updateScreenDto.LocationId, out int locationId) ||
                    !int.TryParse(updateScreenDto.AgencyId, out int agencyId))
                {
                    return BadRequest(new ApiResponse<ScreenDto>
                    {
                        Success = false,
                        Message = "Invalid ID format"
                    });
                }

                screen.Name = updateScreenDto.Name;
                screen.LocationId = locationId;
                screen.AgencyId = agencyId;
                screen.DepartmentId = !string.IsNullOrEmpty(updateScreenDto.DepartmentId) 
                    ? int.Parse(updateScreenDto.DepartmentId) 
                    : null;
                screen.ScreenType = updateScreenDto.ScreenType ?? string.Empty;
                screen.LastUpdated = DateTime.UtcNow;
                screen.IsOnline = updateScreenDto.IsOnline;
                screen.StatusMessage = updateScreenDto.StatusMessage ?? string.Empty;
                screen.MACAddress = updateScreenDto.MACAddress ?? string.Empty;

                _context.Screens.Update(screen);
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

        // DELETE: api/screen/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteScreen(int id)
        {
            try
            {
                var screen = await _context.Screens.FindAsync(id);
                if (screen == null)
                {
                    var notFoundResponse = new ApiResponse<bool>
                    {
                        Success = false,
                        Message = $"Screen with ID {id} not found",
                        Data = false
                    };
                    notFoundResponse.Errors.Add($"Screen with ID {id} not found");
                    return NotFound(notFoundResponse);
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
                var response = new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Internal server error",
                    Data = false
                };
                response.Errors.Add(ex.Message);
                return StatusCode(500, response);
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
    }
}
