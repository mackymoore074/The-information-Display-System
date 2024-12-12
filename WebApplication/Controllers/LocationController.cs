using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ClassLibrary;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Location;


namespace TheWebApplication.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<LocationController> _logger;

        public LocationController(ClassDBContext context, ILogger<LocationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/location
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
                        DateCreated = l.DateCreated
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<LocationDto>>
                {
                    Success = true,
                    Message = "Locations retrieved successfully",
                    Data = locations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetLocations: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<IEnumerable<LocationDto>>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        // GET: api/location/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<LocationDto>>> GetLocation(int id)
        {
            try
            {
                var location = await _context.Locations
                    .Include(l => l.Departments)
                    .Include(l => l.Screens)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (location == null)
                    return NotFound(new ApiResponse<LocationDto>
                    {
                        Success = false,
                        Message = "Location not found",
                        Errors = new List<string> { $"Location with ID {id} not found" }
                    });

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
                    Message = "Location retrieved successfully",
                    Data = locationDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetLocation: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        // POST: api/location
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<LocationDto>>> CreateLocation([FromBody] CreateLocationDto createLocationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });

            try
            {
                var location = new Location
                {
                    Name = createLocationDto.Name,
                    Address = createLocationDto.Address,
                    DateCreated = DateTime.UtcNow
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

                return CreatedAtAction(nameof(GetLocation), new { id = location.Id },
                    new ApiResponse<LocationDto>
                    {
                        Success = true,
                        Message = "Location created successfully",
                        Data = locationDto
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateLocation: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        // PUT: api/location/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<LocationDto>>> UpdateLocation(int id, [FromBody] UpdateLocationDto updateLocationDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });

            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                    return NotFound(new ApiResponse<LocationDto>
                    {
                        Success = false,
                        Message = "Location not found",
                        Errors = new List<string> { $"Location with ID {id} not found" }
                    });

                location.Name = updateLocationDto.Name;
                location.Address = updateLocationDto.Address;

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
                    Message = "Location updated successfully",
                    Data = locationDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateLocation: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        // DELETE: api/location/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteLocation(int id)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Location not found",
                        Errors = new List<string> { $"Location with ID {id} not found" }
                    });

                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Location deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteLocation: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }
    }
}
