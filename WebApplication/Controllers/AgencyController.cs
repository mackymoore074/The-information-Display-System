using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.DtoModels.Agency;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AgencyController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<AgencyController> _logger;

        public AgencyController(ClassDBContext context, ILogger<AgencyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<AgencyDto>>>> GetAgencies()
        {
            try
            {
                var agencies = await _context.Agencies
                    .Include(a => a.Location)
                    .Select(a => new AgencyDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        AdminId = a.AdminId,
                        LocationId = a.LocationId,
                        LocationName = a.Location.Name,
                        DateCreated = a.DateCreated
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<AgencyDto>>
                {
                    Success = true,
                    Message = agencies.Any() ? "Agencies retrieved successfully." : "No agencies found.",
                    Data = agencies
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetAgencies", ex);
                return StatusCode(500, new ApiResponse<IEnumerable<AgencyDto>>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<AgencyDto>>> GetAgency(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<AgencyDto>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Data = null
                });
            }

            try
            {
                var agency = await _context.Agencies
                    .Include(a => a.Location)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (agency == null)
                {
                    return NotFound(new ApiResponse<AgencyDto>
                    {
                        Success = false,
                        Message = $"Agency with ID {id} not found.",
                        Data = null
                    });
                }

                var agencyDto = new AgencyDto
                {
                    Id = agency.Id,
                    Name = agency.Name,
                    Description = agency.Description,
                    AdminId = agency.AdminId,
                    LocationId = agency.LocationId,
                    LocationName = agency.Location.Name,
                    DateCreated = agency.DateCreated
                };

                return Ok(new ApiResponse<AgencyDto>
                {
                    Success = true,
                    Message = "Agency retrieved successfully.",
                    Data = agencyDto
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetAgency", ex);
                return StatusCode(500, new ApiResponse<AgencyDto>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AgencyDto>>> CreateAgency([FromBody] CreateAgencyDto createAgencyDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return BadRequest(new ApiResponse<AgencyDto>
                {
                    Success = false,
                    Message = "Validation errors occurred.",
                    Errors = errors
                });
            }

            try
            {
                var agency = new Agency
                {
                    Name = createAgencyDto.Name,
                    Description = createAgencyDto.Description,
                    AdminId = 1,
                    LocationId = createAgencyDto.LocationId,
                    DateCreated = DateTime.UtcNow
                };

                _context.Agencies.Add(agency);
                await _context.SaveChangesAsync();

                var agencyDto = new AgencyDto
                {
                    Id = agency.Id,
                    Name = agency.Name,
                    Description = agency.Description,
                    AdminId = agency.AdminId,
                    LocationId = agency.LocationId,
                    LocationName = (await _context.Locations.FindAsync(agency.LocationId))?.Name,
                    DateCreated = agency.DateCreated
                };

                return CreatedAtAction(nameof(GetAgency), new { id = agency.Id }, new ApiResponse<AgencyDto>
                {
                    Success = true,
                    Message = "Agency created successfully.",
                    Data = agencyDto
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in CreateAgency", ex);
                return StatusCode(500, new ApiResponse<AgencyDto>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<AgencyDto>>> UpdateAgency(int id, [FromBody] UpdateAgencyDto updateAgencyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<AgencyDto>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Data = null
                });
            }

            try
            {
                var agency = await _context.Agencies.FindAsync(id);
                if (agency == null)
                {
                    return NotFound(new ApiResponse<AgencyDto>
                    {
                        Success = false,
                        Message = $"Agency with ID {id} not found.",
                        Data = null
                    });
                }

                agency.Name = updateAgencyDto.Name;
                agency.Description = updateAgencyDto.Description;
                agency.LocationId = updateAgencyDto.LocationId;

                _context.Entry(agency).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var agencyDto = new AgencyDto
                {
                    Id = agency.Id,
                    Name = agency.Name,
                    Description = agency.Description,
                    AdminId = agency.AdminId,
                    LocationId = agency.LocationId,
                    LocationName = (await _context.Locations.FindAsync(agency.LocationId))?.Name,
                    DateCreated = agency.DateCreated
                };

                return Ok(new ApiResponse<AgencyDto>
                {
                    Success = true,
                    Message = "Agency updated successfully.",
                    Data = agencyDto
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in UpdateAgency", ex);
                return StatusCode(500, new ApiResponse<AgencyDto>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteAgency(int id)
        {
            try
            {
                var agency = await _context.Agencies.FindAsync(id);
                if (agency == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Agency with ID {id} not found.",
                        Data = null
                    });
                }

                _context.Agencies.Remove(agency);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Agency deleted successfully.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteAgency", ex);
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

                await _context.ErrorLogs.AddAsync(errorLog);
                await _context.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                _logger.LogError($"Failed to log error to database: {logEx.Message}", logEx);
                // Don't throw - we don't want logging failures to affect the main operation
            }
        }
    }
}
