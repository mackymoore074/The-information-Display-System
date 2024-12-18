using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.DtoModels.Location;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<LocationController> _logger;

        public LocationController(ClassDBContext context, ILogger<LocationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<LocationDto>>>> GetLocations()
        {
            try
            {
                var locations = await _context.Locations
                    .Select(l => new LocationDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Address = l.Address,
                        DateCreated = l.DateCreated,
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<LocationDto>>
                {
                    Success = true,
                    Message = locations.Any() ? "Locations retrieved successfully." : "No locations found.",
                    Data = locations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetLocations: {ex.Message}", ex);
                await LogErrorToDatabaseAsync("Error in GetLocations", ex);
                return StatusCode(500, new ApiResponse<IEnumerable<LocationDto>>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<LocationDto>>> GetLocation(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Data = null
                });
            }

            try
            {
                var location = await _context.Locations
                    .Include(l => l.Screens)
                    .Include(l => l.Departments)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (location == null)
                {
                    return NotFound(new ApiResponse<LocationDto>
                    {
                        Success = false,
                        Message = $"Location with ID {id} not found.",
                        Data = null
                    });
                }

                var locationDto = new LocationDto
                {
                    Id = location.Id,
                    Name = location.Name,
                    Address = location.Address,
                    DateCreated = location.DateCreated,
                };

                return Ok(new ApiResponse<LocationDto>
                {
                    Success = true,
                    Message = "Location retrieved successfully.",
                    Data = locationDto
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetLocation", ex);
                return StatusCode(500, new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<LocationDto>>> CreateLocation([FromBody] CreateLocationDto createLocationDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return BadRequest(new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Validation errors occurred.",
                    Errors = errors
                });
            }

            try
            {
                var location = new Location
                {
                    Name = createLocationDto.Name,
                    Address = createLocationDto.Address,
                    DateCreated = DateTime.UtcNow,
                    AdminId = 1
                };

                _context.Locations.Add(location);
                await _context.SaveChangesAsync();

                var locationDto = new LocationDto
                {
                    Id = location.Id,
                    Name = location.Name,
                    Address = location.Address,
                    DateCreated = location.DateCreated
                };

                return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, new ApiResponse<LocationDto>
                {
                    Success = true,
                    Message = "Location created successfully.",
                    Data = locationDto
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in CreateLocation", ex);
                return StatusCode(500, new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<LocationDto>>> UpdateLocation(int id, [FromBody] UpdateLocationDto updateLocationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Data = null
                });
            }

            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                {
                    return NotFound(new ApiResponse<LocationDto>
                    {
                        Success = false,
                        Message = $"Location with ID {id} not found.",
                        Data = null
                    });
                }

                location.Name = updateLocationDto.Name;
                location.Address = updateLocationDto.Address;

                _context.Entry(location).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var locationDto = new LocationDto
                {
                    Id = location.Id,
                    Name = location.Name,
                    Address = location.Address,
                    DateCreated = location.DateCreated
                };

                return Ok(new ApiResponse<LocationDto>
                {
                    Success = true,
                    Message = "Location updated successfully.",
                    Data = locationDto
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in UpdateLocation", ex);
                return StatusCode(500, new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteLocation(int id)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Location with ID {id} not found.",
                        Data = null
                    });
                }

                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Location deleted successfully.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteLocation", ex);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
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
