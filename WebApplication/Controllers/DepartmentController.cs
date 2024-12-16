using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.DtoModels.Department;
using ClassLibrary.Models;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(ClassDBContext context, ILogger<DepartmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/department
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            try
            {
                var departments = await _context.Departments
                    .Select(d => new DepartmentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        DateCreated = d.DateCreated,
                        AgencyId = d.AgencyId,
                        LocationId = d.LocationId
                    })
                    .ToListAsync();

                return Ok(departments);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetDepartments", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/department/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);

                if (department == null)
                    return NotFound($"Department with ID {id} not found.");

                var departmentDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    DateCreated = department.DateCreated,
                    AgencyId = department.AgencyId,
                    LocationId = department.LocationId
                };

                return Ok(departmentDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetDepartment", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: api/department
        [HttpPost]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto createDepartmentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var department = new Department
                {
                    Name = createDepartmentDto.Name,
                    Description = createDepartmentDto.Description,
                    AgencyId = createDepartmentDto.AgencyId,
                    LocationId = createDepartmentDto.LocationId,
                    DateCreated = DateTime.UtcNow
                };

                _context.Departments.Add(department);
                await _context.SaveChangesAsync();

                var departmentDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    DateCreated = department.DateCreated,
                    AgencyId = department.AgencyId,
                    LocationId = department.LocationId
                };

                return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, departmentDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in CreateDepartment", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // PUT: api/department/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<DepartmentDto>> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto updateDepartmentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                    return NotFound($"Department with ID {id} not found.");

                department.Name = updateDepartmentDto.Name;
                department.Description = updateDepartmentDto.Description;
                department.AgencyId = updateDepartmentDto.AgencyId;
                department.LocationId = updateDepartmentDto.LocationId;

                _context.Departments.Update(department);
                await _context.SaveChangesAsync();

                var departmentDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    DateCreated = department.DateCreated,
                    AgencyId = department.AgencyId,
                    LocationId = department.LocationId
                };

                return Ok(departmentDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in UpdateDepartment", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // DELETE: api/department/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                    return NotFound($"Department with ID {id} not found.");

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteDepartment", ex);
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
