using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ClassLibrary;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Department;
using ClassLibrary.DtoModels.Admin;
using ClassLibrary.DtoModels.Common;



namespace TheWebApplication.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(ClassDBContext context, ILogger<DepartmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentDto>>>> GetDepartments()
        {
            try
            {
                var departments = await _context.Departments
                    .Include(d => d.Agency)
                    .Include(d => d.Location)
                    .Select(d => new DepartmentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        DateCreated = d.DateCreated,
                        AgencyId = d.AgencyId,
                        AgencyName = d.Agency != null ? d.Agency.Name : null,
                        LocationId = d.LocationId,
                        LocationName = d.Location.Name
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<DepartmentDto>>
                {
                    Success = true,
                    Message = "Departments retrieved successfully",
                    Data = departments
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDepartments: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<IEnumerable<DepartmentDto>>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetDepartment(int id)
        {
            try
            {
                var department = await _context.Departments
                    .Include(d => d.Agency)
                    .Include(d => d.Location)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (department == null)
                {
                    return NotFound(new ApiResponse<DepartmentDto>
                    {
                        Success = false,
                        Message = "Department not found",
                        Errors = new List<string> { $"Department with ID {id} not found" }
                    });
                }

                var departmentDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    DateCreated = department.DateCreated,
                    AgencyId = department.AgencyId,
                    AgencyName = department.Agency?.Name,
                    LocationId = department.LocationId,
                    LocationName = department.Location.Name
                };

                return Ok(new ApiResponse<DepartmentDto>
                {
                    Success = true,
                    Message = "Department retrieved successfully",
                    Data = departmentDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDepartment: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> CreateDepartment([FromBody] CreateDepartmentDto createDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            try
            {
                // Validate LocationId exists
                var location = await _context.Locations.FindAsync(createDepartmentDto.LocationId);
                if (location == null)
                {
                    return BadRequest(new ApiResponse<DepartmentDto>
                    {
                        Success = false,
                        Message = "Validation failed",
                        Errors = new List<string> { $"Location with ID {createDepartmentDto.LocationId} not found" }
                    });
                }

                // Validate AgencyId exists if provided
                if (createDepartmentDto.AgencyId.HasValue)
                {
                    var agency = await _context.Agencies.FindAsync(createDepartmentDto.AgencyId.Value);
                    if (agency == null)
                    {
                        return BadRequest(new ApiResponse<DepartmentDto>
                        {
                            Success = false,
                            Message = "Validation failed",
                            Errors = new List<string> { $"Agency with ID {createDepartmentDto.AgencyId} not found" }
                        });
                    }
                }

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

                // Load related entities after save
                await _context.Entry(department)
                    .Reference(d => d.Location)
                    .LoadAsync();

                if (department.AgencyId.HasValue)
                {
                    await _context.Entry(department)
                        .Reference(d => d.Agency)
                        .LoadAsync();
                }

                var departmentDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    DateCreated = department.DateCreated,
                    AgencyId = department.AgencyId,
                    AgencyName = department.Agency?.Name,
                    LocationId = department.LocationId,
                    LocationName = department.Location.Name
                };

                return CreatedAtAction(nameof(GetDepartment), new { id = department.Id },
                    new ApiResponse<DepartmentDto>
                    {
                        Success = true,
                        Message = "Department created successfully",
                        Data = departmentDto
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateDepartment: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto updateDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                });
            }

            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                {
                    return NotFound(new ApiResponse<DepartmentDto>
                    {
                        Success = false,
                        Message = "Department not found",
                        Errors = new List<string> { $"Department with ID {id} not found" }
                    });
                }

                department.Name = updateDepartmentDto.Name;
                department.Description = updateDepartmentDto.Description;
                department.AgencyId = updateDepartmentDto.AgencyId;
                department.LocationId = updateDepartmentDto.LocationId;

                await _context.SaveChangesAsync();

                await _context.Entry(department)
                    .Reference(d => d.Agency)
                    .LoadAsync();
                await _context.Entry(department)
                    .Reference(d => d.Location)
                    .LoadAsync();

                var departmentDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    DateCreated = department.DateCreated,
                    AgencyId = department.AgencyId,
                    AgencyName = department.Agency?.Name,
                    LocationId = department.LocationId,
                    LocationName = department.Location.Name
                };

                return Ok(new ApiResponse<DepartmentDto>
                {
                    Success = true,
                    Message = "Department updated successfully",
                    Data = departmentDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateDepartment: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteDepartment(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Department not found",
                        Errors = new List<string> { $"Department with ID {id} not found" }
                    });
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Department deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteDepartment: {ex.Message}", ex);
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
