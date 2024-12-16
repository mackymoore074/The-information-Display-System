using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SystemModels.Models;
using SystemModels.DtoModels.Employee;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.DtoModels.Employee;
using ClassLibrary.Models;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmployeeController : ControllerBase
    {
        private readonly InfoDbContext _context;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(InfoDbContext context, ILogger<EmployeeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/employee
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
        {
            try
            {
                var employees = await _context.Employees
                    .Select(e => new EmployeeDto
                    {
                        Id = e.Id,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        Email = e.Email,
                        DateCreated = e.DateCreated,
                        AdminId = e.AdminId,
                        DepartmentId = e.DepartmentId
                    })
                    .ToListAsync();

                return Ok(employees);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetEmployees", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/employee/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
        {
            try
            {
                var employee = await _context.Employees
                    .Where(e => e.Id == id)
                    .FirstOrDefaultAsync();

                if (employee == null)
                    return NotFound($"Employee with ID {id} not found.");

                var employeeDto = new EmployeeDto
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    DateCreated = employee.DateCreated,
                    AdminId = employee.AdminId,
                    DepartmentId = employee.DepartmentId
                };

                return Ok(employeeDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetEmployee", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: api/employee
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee([FromBody] CreateEmployeeDto createEmployeeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var employee = new Employee
                {
                    FirstName = createEmployeeDto.FirstName,
                    LastName = createEmployeeDto.LastName,
                    Email = createEmployeeDto.Email,
                    AdminId = createEmployeeDto.AdminId,
                    DepartmentId = createEmployeeDto.DepartmentId,
                    DateCreated = DateTime.UtcNow // Set the current date and time
                };

                // Assign departments


                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                var employeeDto = new EmployeeDto
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    DateCreated = employee.DateCreated,
                    AdminId = employee.AdminId,
                    DepartmentId = employee.DepartmentId
                };

                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employeeDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in CreateEmployee", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // PUT: api/employee/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<EmployeeDto>> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                    return NotFound($"Employee with ID {id} not found.");

                employee.FirstName = updateEmployeeDto.FirstName;
                employee.LastName = updateEmployeeDto.LastName;
                employee.Email = updateEmployeeDto.Email;
                employee.DepartmentId = updateEmployeeDto.DepartmentId;

                // You can update other fields similarly if needed, but I assume Email, AdminId, and DepartmentId are not meant to be updated

                _context.Employees.Update(employee);
                await _context.SaveChangesAsync();

                var employeeDto = new EmployeeDto
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    DepartmentId = employee.DepartmentId
                };

                return Ok(employeeDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in UpdateEmployee", ex);
                return StatusCode(500, "Internal server error.");
            }
        }



        // DELETE: api/employee/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                    return NotFound($"Employee with ID {id} not found.");

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteEmployee", ex);
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
