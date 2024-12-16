using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemModels.Models;
using SystemModels.DtoModels.Location;
using SystemModels;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.DtoModels.Location;
using ClassLibrary.Models;

namespace TheWebApplication.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LocationController : ControllerBase
    {
        private readonly InfoDbContext _context;
        private readonly ILogger<LocationController> _logger;

        public LocationController(InfoDbContext context, ILogger<LocationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/location
        [HttpGet("get-location")]
        public async Task<ActionResult<IEnumerable<LocationDto>>> GetLocations()
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

                return Ok(locations);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetLocations", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/location/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDto>> GetLocation(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var location = await _context.Locations
                    .Include(l => l.Screens)
                    .Include(l => l.Departments)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (location == null)
                {
                    return NotFound($"Location with ID {id} not found.");
                }

                var locationDto = new LocationDto
                {
                    Id = location.Id,
                    Name = location.Name,
                    Address = location.Address,
                    DateCreated = location.DateCreated,
                };

                return Ok(locationDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetLocation", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: api/location
        [HttpPost]
        public async Task<ActionResult<LocationDto>> CreateLocation([FromBody] CreateLocationDto createLocationDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return BadRequest(errors);
            }

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

                return CreatedAtAction(nameof(GetLocation), new { id = location.Id }, locationDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in CreateLocation", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // PUT: api/location/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLocation(int id, [FromBody] UpdateLocationDto updateLocationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                {
                    return NotFound($"Location with ID {id} not found.");
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

                return Ok(locationDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in UpdateLocation", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // DELETE: api/location/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                {
                    return NotFound($"Location with ID {id} not found.");
                }

                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteLocation", ex);
                return StatusCode(500, "Internal server error.");
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
