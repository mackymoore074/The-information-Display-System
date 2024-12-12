using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassLibrary.DtoModels.Screen;
using ClassLibrary.Models;
using ClassLibrary;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.DtoModels.Common;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Authorize]
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
        public async Task<ActionResult<ApiResponse<IEnumerable<ScreenDto>>>> GetScreens()
        {
            try
            {
                var screens = await _context.Screens
                    .Include(s => s.Agency)
                    .Include(s => s.Department)
                    .Include(s => s.Location)
                    .Select(s => new ScreenDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        ScreenType = s.ScreenType,
                        IsOnline = s.IsOnline,
                        StatusMessage = s.StatusMessage,
                        MACAddress = s.MACAddress,
                        DateCreated = s.DateCreated,
                        LastCheckedIn = s.LastCheckedIn,
                        LastUpdated = s.LastUpdated,
                        AgencyId = s.AgencyId,
                        AgencyName = s.Agency.Name,
                        DepartmentId = s.DepartmentId,
                        DepartmentName = s.Department != null ? s.Department.Name : null,
                        LocationId = s.LocationId,
                        LocationName = s.Location != null ? s.Location.Name : null
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<ScreenDto>>
                {
                    Success = true,
                    Message = "Screens retrieved successfully",
                    Data = screens
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetScreens: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<IEnumerable<ScreenDto>>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
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
                    .Include(s => s.Agency)
                    .Include(s => s.Department)
                    .Include(s => s.Location)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (screen == null)
                {
                    return NotFound(new ApiResponse<ScreenDto>
                    {
                        Success = false,
                        Message = $"Screen with ID {id} not found"
                    });
                }

                var screenDto = new ScreenDto
                {
                    Id = screen.Id,
                    Name = screen.Name,
                    ScreenType = screen.ScreenType,
                    IsOnline = screen.IsOnline,
                    StatusMessage = screen.StatusMessage,
                    MACAddress = screen.MACAddress,
                    DateCreated = screen.DateCreated,
                    LastCheckedIn = screen.LastCheckedIn,
                    LastUpdated = screen.LastUpdated,
                    AgencyId = screen.AgencyId,
                    AgencyName = screen.Agency?.Name,
                    DepartmentId = screen.DepartmentId,
                    DepartmentName = screen.Department?.Name,
                    LocationId = screen.LocationId,
                    LocationName = screen.Location?.Name
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
                _logger.LogError($"Error in GetScreen: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        // POST: api/screen
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<ScreenDto>>> CreateScreen([FromBody] CreateScreenDto createScreenDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });

            try
            {
                // Validate agency exists
                var agency = await _context.Agencies.FindAsync(createScreenDto.AgencyId);
                if (agency == null)
                    return BadRequest(new ApiResponse<ScreenDto>
                    {
                        Success = false,
                        Message = "Invalid agency",
                        Errors = new List<string> { $"Agency with ID {createScreenDto.AgencyId} not found" }
                    });

                // Validate department if provided
                if (createScreenDto.DepartmentId.HasValue)
                {
                    var department = await _context.Departments
                        .FirstOrDefaultAsync(d => d.Id == createScreenDto.DepartmentId && d.AgencyId == createScreenDto.AgencyId);
                    if (department == null)
                        return BadRequest(new ApiResponse<ScreenDto>
                        {
                            Success = false,
                            Message = "Invalid department",
                            Errors = new List<string> { 
                                $"Department with ID {createScreenDto.DepartmentId} not found or does not belong to the specified agency" 
                            }
                        });
                }

                // Validate location if provided
                if (createScreenDto.LocationId.HasValue)
                {
                    var location = await _context.Locations.FindAsync(createScreenDto.LocationId);
                    if (location == null)
                        return BadRequest(new ApiResponse<ScreenDto>
                        {
                            Success = false,
                            Message = "Invalid location",
                            Errors = new List<string> { $"Location with ID {createScreenDto.LocationId} not found" }
                        });
                }

                var screen = new Screen
                {
                    Name = createScreenDto.Name,
                    LocationId = createScreenDto.LocationId,
                    DepartmentId = createScreenDto.DepartmentId,
                    AgencyId = createScreenDto.AgencyId,
                    ScreenType = createScreenDto.ScreenType,
                    IsOnline = createScreenDto.IsOnline,
                    StatusMessage = createScreenDto.StatusMessage,
                    MACAddress = createScreenDto.MACAddress,
                    DateCreated = DateTime.UtcNow,
                    LastCheckedIn = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                };

                _context.Screens.Add(screen);
                await _context.SaveChangesAsync();

                // Reload to get related entity names
                await _context.Entry(screen)
                    .Reference(s => s.Agency)
                    .LoadAsync();
                if (screen.DepartmentId.HasValue)
                    await _context.Entry(screen)
                        .Reference(s => s.Department)
                        .LoadAsync();
                if (screen.LocationId.HasValue)
                    await _context.Entry(screen)
                        .Reference(s => s.Location)
                        .LoadAsync();

                var screenDto = new ScreenDto
                {
                    Id = screen.Id,
                    Name = screen.Name,
                    ScreenType = screen.ScreenType,
                    IsOnline = screen.IsOnline,
                    StatusMessage = screen.StatusMessage,
                    MACAddress = screen.MACAddress,
                    DateCreated = screen.DateCreated,
                    LastCheckedIn = screen.LastCheckedIn,
                    LastUpdated = screen.LastUpdated,
                    AgencyId = screen.AgencyId,
                    AgencyName = screen.Agency?.Name,
                    DepartmentId = screen.DepartmentId,
                    DepartmentName = screen.Department?.Name,
                    LocationId = screen.LocationId,
                    LocationName = screen.Location?.Name
                };

                return CreatedAtAction(nameof(GetScreen), new { id = screen.Id }, 
                    new ApiResponse<ScreenDto>
                    {
                        Success = true,
                        Message = "Screen created successfully",
                        Data = screenDto
                    });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError($"Database update error: {dbEx.Message}", dbEx);
                return StatusCode(500, new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Database update failed",
                    Errors = new List<string> { dbEx.InnerException?.Message ?? dbEx.Message }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateScreen: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        // PUT: api/screen/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<ScreenDto>>> UpdateScreen(int id, [FromBody] UpdateScreenDto updateScreenDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });

            try
            {
                var screen = await _context.Screens
                    .Include(s => s.Agency)
                    .Include(s => s.Department)
                    .Include(s => s.Location)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (screen == null)
                    return NotFound(new ApiResponse<ScreenDto>
                    {
                        Success = false,
                        Message = "Screen not found",
                        Errors = new List<string> { $"Screen with ID {id} not found" }
                    });

                // Validate agency exists
                if (updateScreenDto.AgencyId != screen.AgencyId)
                {
                    var agency = await _context.Agencies.FindAsync(updateScreenDto.AgencyId);
                    if (agency == null)
                        return BadRequest(new ApiResponse<ScreenDto>
                        {
                            Success = false,
                            Message = "Invalid agency",
                            Errors = new List<string> { $"Agency with ID {updateScreenDto.AgencyId} not found" }
                        });
                }

                // Validate department if provided
                if (updateScreenDto.DepartmentId.HasValue)
                {
                    var department = await _context.Departments
                        .FirstOrDefaultAsync(d => d.Id == updateScreenDto.DepartmentId);
                    if (department == null)
                        return BadRequest(new ApiResponse<ScreenDto>
                        {
                            Success = false,
                            Message = "Invalid department",
                            Errors = new List<string> { $"Department with ID {updateScreenDto.DepartmentId} not found" }
                        });
                }

                // Validate location if provided
                if (updateScreenDto.LocationId.HasValue)
                {
                    var location = await _context.Locations.FindAsync(updateScreenDto.LocationId);
                    if (location == null)
                        return BadRequest(new ApiResponse<ScreenDto>
                        {
                            Success = false,
                            Message = "Invalid location",
                            Errors = new List<string> { $"Location with ID {updateScreenDto.LocationId} not found" }
                        });
                }

                screen.Name = updateScreenDto.Name;
                screen.DepartmentId = updateScreenDto.DepartmentId;
                screen.LocationId = updateScreenDto.LocationId;
                screen.AgencyId = updateScreenDto.AgencyId;
                screen.ScreenType = updateScreenDto.ScreenType;
                screen.IsOnline = updateScreenDto.IsOnline;
                screen.StatusMessage = updateScreenDto.StatusMessage;
                screen.MACAddress = updateScreenDto.MACAddress;
                screen.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reload to get updated related entity names
                await _context.Entry(screen)
                    .Reference(s => s.Agency)
                    .LoadAsync();
                if (screen.DepartmentId.HasValue)
                    await _context.Entry(screen)
                        .Reference(s => s.Department)
                        .LoadAsync();
                if (screen.LocationId.HasValue)
                    await _context.Entry(screen)
                        .Reference(s => s.Location)
                        .LoadAsync();

                var screenDto = new ScreenDto
                {
                    Id = screen.Id,
                    Name = screen.Name,
                    ScreenType = screen.ScreenType,
                    IsOnline = screen.IsOnline,
                    StatusMessage = screen.StatusMessage,
                    MACAddress = screen.MACAddress,
                    DateCreated = screen.DateCreated,
                    LastCheckedIn = screen.LastCheckedIn,
                    LastUpdated = screen.LastUpdated,
                    AgencyId = screen.AgencyId,
                    AgencyName = screen.Agency?.Name,
                    DepartmentId = screen.DepartmentId,
                    DepartmentName = screen.Department?.Name,
                    LocationId = screen.LocationId,
                    LocationName = screen.Location?.Name
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
                _logger.LogError($"Error in UpdateScreen: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        // DELETE: api/screen/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteScreen(int id)
        {
            try
            {
                var screen = await _context.Screens.FindAsync(id);
                if (screen == null)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Screen not found",
                        Errors = new List<string> { $"Screen with ID {id} not found" }
                    });

                _context.Screens.Remove(screen);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Screen deleted successfully"
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteScreen", ex);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
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
    }
}
