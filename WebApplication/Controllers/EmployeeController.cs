using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Employee;
using ClassLibrary;
using Microsoft.AspNetCore.Authorization;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(ClassDBContext context, ILogger<EmployeeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<EmployeeDto>>>> GetEmployees()
        {
            try
            {
                var employees = await _context.Employees
                    .Include(e => e.Department)
                    .Select(e => new EmployeeDto
                    {
                        Id = e.Id,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        Email = e.Email,
                        DateCreated = e.DateCreated,
                        DepartmentId = e.DepartmentId,
                        DepartmentName = e.Department.Name
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<EmployeeDto>>
                {
                    Success = true,
                    Message = "Employees retrieved successfully",
                    Data = employees
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetEmployees: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<IEnumerable<EmployeeDto>>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> GetEmployee(int id)
        {
            try
            {
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (employee == null)
                    return NotFound(new ApiResponse<EmployeeDto>
                    {
                        Success = false,
                        Message = "Employee not found",
                        Errors = new List<string> { $"Employee with ID {id} not found" }
                    });

                var employeeDto = new EmployeeDto
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    DateCreated = employee.DateCreated,
                    DepartmentId = employee.DepartmentId,
                    DepartmentName = employee.Department.Name
                };

                return Ok(new ApiResponse<EmployeeDto>
                {
                    Success = true,
                    Message = "Employee retrieved successfully",
                    Data = employeeDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetEmployee: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<EmployeeDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> CreateEmployee([FromBody] CreateEmployeeDto createEmployeeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<EmployeeDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });

            try
            {
                var department = await _context.Departments.FindAsync(createEmployeeDto.DepartmentId);
                if (department == null)
                    return BadRequest(new ApiResponse<EmployeeDto>
                    {
                        Success = false,
                        Message = "Invalid department",
                        Errors = new List<string> { $"Department with ID {createEmployeeDto.DepartmentId} not found" }
                    });

                var employee = new Employee
                {
                    FirstName = createEmployeeDto.FirstName,
                    LastName = createEmployeeDto.LastName,
                    Email = createEmployeeDto.Email,
                    DateCreated = DateTime.UtcNow,
                    DepartmentId = createEmployeeDto.DepartmentId
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                await _context.Entry(employee)
                    .Reference(e => e.Department)
                    .LoadAsync();

                var employeeDto = new EmployeeDto
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    DateCreated = employee.DateCreated,
                    DepartmentId = employee.DepartmentId,
                    DepartmentName = employee.Department.Name
                };

                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id },
                    new ApiResponse<EmployeeDto>
                    {
                        Success = true,
                        Message = "Employee created successfully",
                        Data = employeeDto
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateEmployee: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<EmployeeDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<EmployeeDto>>> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto updateEmployeeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<EmployeeDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });

            try
            {
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (employee == null)
                    return NotFound(new ApiResponse<EmployeeDto>
                    {
                        Success = false,
                        Message = "Employee not found",
                        Errors = new List<string> { $"Employee with ID {id} not found" }
                    });

                if (employee.DepartmentId != updateEmployeeDto.DepartmentId)
                {
                    var department = await _context.Departments.FindAsync(updateEmployeeDto.DepartmentId);
                    if (department == null)
                        return BadRequest(new ApiResponse<EmployeeDto>
                        {
                            Success = false,
                            Message = "Invalid department",
                            Errors = new List<string> { $"Department with ID {updateEmployeeDto.DepartmentId} not found" }
                        });
                }

                employee.FirstName = updateEmployeeDto.FirstName;
                employee.LastName = updateEmployeeDto.LastName;
                employee.Email = updateEmployeeDto.Email;
                employee.DepartmentId = updateEmployeeDto.DepartmentId;

                await _context.SaveChangesAsync();

                await _context.Entry(employee)
                    .Reference(e => e.Department)
                    .LoadAsync();

                var employeeDto = new EmployeeDto
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                    DateCreated = employee.DateCreated,
                    DepartmentId = employee.DepartmentId,
                    DepartmentName = employee.Department.Name
                };

                return Ok(new ApiResponse<EmployeeDto>
                {
                    Success = true,
                    Message = "Employee updated successfully",
                    Data = employeeDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateEmployee: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<EmployeeDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Employee not found",
                        Errors = new List<string> { $"Employee with ID {id} not found" }
                    });

                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Employee deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteEmployee: {ex.Message}", ex);
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
