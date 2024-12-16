using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemModels.Models;
using SystemModels.DtoModels.Agency;
using SystemModels;
using ClassLibrary.DtoModels.Agency;
using ClassLibrary.Models;

namespace TheWebApplication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencyController : ControllerBase
    {
        private readonly InfoDbContext _context;
        private readonly ILogger<AgencyController> _logger;

        public AgencyController(InfoDbContext context, ILogger<AgencyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/agency
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AgencyDto>>> GetAgencies()
        {
            try
            {
                var agencies = await _context.Agencies
                    .Select(a => new AgencyDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        AdminId = a.AdminId,
                        LocationId = a.LocationId
                    })
                    .ToListAsync();

                return Ok(agencies);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetAgencies", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/agency/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AgencyDto>> GetAgency(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var agency = await _context.Agencies
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (agency == null)
                {
                    return NotFound($"Agency with ID {id} not found.");
                }

                var agencyDto = new AgencyDto
                {
                    Id = agency.Id,
                    Name = agency.Name,
                    Description = agency.Description,
                    AdminId = agency.AdminId,
                    LocationId = agency.LocationId
                };

                return Ok(agencyDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetAgency", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: api/agency
        [HttpPost]
        public async Task<ActionResult<AgencyDto>> CreateAgency([FromBody] CreateAgencyDto createAgencyDto)
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
                var agency = new Agency
                {
                    Name = createAgencyDto.Name,
                    Description = createAgencyDto.Description,
                    AdminId = createAgencyDto.AdminId,
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
                    LocationId = agency.LocationId
                };

                return CreatedAtAction(nameof(GetAgency), new { id = agency.Id }, agencyDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in CreateAgency", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // PUT: api/agency/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAgency(int id, [FromBody] UpdateAgencyDto updateAgencyDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var agency = await _context.Agencies.FindAsync(id);
                if (agency == null)
                {
                    return NotFound($"Agency with ID {id} not found.");
                }

                agency.Name = updateAgencyDto.Name;
                agency.Description = updateAgencyDto.Description;

                _context.Entry(agency).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var agencyDto = new AgencyDto
                {
                    Id = agency.Id,
                    Name = agency.Name,
                    Description = agency.Description,
                    AdminId = agency.AdminId,
                    LocationId = agency.LocationId
                };

                return Ok(agencyDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in UpdateAgency", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // DELETE: api/agency/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgency(int id)
        {
            try
            {
                var agency = await _context.Agencies.FindAsync(id);
                if (agency == null)
                {
                    return NotFound($"Agency with ID {id} not found.");
                }

                _context.Agencies.Remove(agency);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteAgency", ex);
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
