using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Employee;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.API;

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

        private int GetCurrentAdminId()
        {
            var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == "AdminId");
            if (adminIdClaim == null)
            {
                throw new UnauthorizedAccessException("Admin ID not found in token");
            }
            return int.Parse(adminIdClaim.Value);
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

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<Employee>>>> GetAll()
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();
                var employees = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Location)
                    .Where(e => e.AdminId == currentAdminId)
                    .OrderBy(e => e.LastName)
                    .ToListAsync();

                return Ok(new ApiResponse<List<Employee>>
                {
                    Success = true,
                    Data = employees
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting employees: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<List<Employee>>
                {
                    Success = false,
                    Message = "Error retrieving employees",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Employee>>> GetById(int id)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .Include(e => e.Location)
                    .FirstOrDefaultAsync(e => e.Id == id && e.AdminId == currentAdminId);

                if (employee == null)
                {
                    return NotFound(new ApiResponse<Employee>
                    {
                        Success = false,
                        Message = "Employee not found"
                    });
                }

                return Ok(new ApiResponse<Employee>
                {
                    Success = true,
                    Data = employee
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting employee: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<Employee>
                {
                    Success = false,
                    Message = "Error retrieving employee",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Employee>>> Create([FromBody] CreateEmployeeDto createDto)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();

                // Check if employee ID already exists
                if (await _context.Employees.AnyAsync(e => e.EmployeeId == createDto.EmployeeId))
                {
                    return BadRequest(new ApiResponse<Employee>
                    {
                        Success = false,
                        Message = "Employee ID already exists",
                        Errors = new List<string> { "This Employee ID is already in use" }
                    });
                }

                var employee = new Employee
                {
                    FirstName = createDto.FirstName,
                    LastName = createDto.LastName,
                    Email = createDto.Email,
                    EmployeeId = createDto.EmployeeId,
                    DepartmentId = createDto.DepartmentId,
                    LocationId = createDto.LocationId,
                    AdminId = currentAdminId,
                    DateCreated = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.Employees.AddAsync(employee);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<Employee>
                {
                    Success = true,
                    Message = "Employee created successfully",
                    Data = employee
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating employee: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<Employee>
                {
                    Success = false,
                    Message = "Error creating employee",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<Employee>>> Update(int id, [FromBody] CreateEmployeeDto updateDto)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Id == id && e.AdminId == currentAdminId);

                if (employee == null)
                {
                    return NotFound(new ApiResponse<Employee>
                    {
                        Success = false,
                        Message = "Employee not found"
                    });
                }

                // Check if updated employee ID conflicts with existing one
                if (await _context.Employees.AnyAsync(e => e.EmployeeId == updateDto.EmployeeId && e.Id != id))
                {
                    return BadRequest(new ApiResponse<Employee>
                    {
                        Success = false,
                        Message = "Employee ID already exists",
                        Errors = new List<string> { "This Employee ID is already in use" }
                    });
                }

                employee.FirstName = updateDto.FirstName;
                employee.LastName = updateDto.LastName;
                employee.Email = updateDto.Email;
                employee.EmployeeId = updateDto.EmployeeId;
                employee.DepartmentId = updateDto.DepartmentId;
                employee.LocationId = updateDto.LocationId;
                employee.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<Employee>
                {
                    Success = true,
                    Message = "Employee updated successfully",
                    Data = employee
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating employee: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<Employee>
                {
                    Success = false,
                    Message = "Error updating employee",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Id == id && e.AdminId == currentAdminId);

                if (employee == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Employee not found"
                    });
                }

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
                _logger.LogError($"Error deleting employee: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting employee",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
}
