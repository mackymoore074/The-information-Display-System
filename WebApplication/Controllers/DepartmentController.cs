using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.DtoModels.Department;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;

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

        private int GetCurrentAdminId()
        {
            var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == "AdminId");
            if (adminIdClaim == null)
            {
                throw new UnauthorizedAccessException("Admin ID not found in token");
            }
            return int.Parse(adminIdClaim.Value);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<DepartmentDto>>>> GetDepartments()
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();

                var departments = await _context.Departments
                    .Include(d => d.Location)
                    .Include(d => d.Agency)
                    .Where(d => d.Location.AdminId == currentAdminId || (d.Agency != null && d.Agency.AdminId == currentAdminId))
                    .Select(d => new DepartmentDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Description = d.Description,
                        AgencyId = d.AgencyId ?? 0,
                        LocationId = d.LocationId,
                        AgencyName = d.Agency.Name,
                        LocationName = d.Location.Name,
                        DateCreated = d.DateCreated
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<DepartmentDto>>
                {
                    Success = true,
                    Message = departments.Any() ? "Departments retrieved successfully." : "No departments found.",
                    Data = departments
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetDepartments", ex);
                return StatusCode(500, new ApiResponse<IEnumerable<DepartmentDto>>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetDepartment(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Data = null
                });
            }

            try
            {
                var department = await _context.Departments
                    .Include(d => d.Location)
                    .Include(d => d.Agency)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (department == null)
                {
                    return NotFound(new ApiResponse<DepartmentDto>
                    {
                        Success = false,
                        Message = $"Department with ID {id} not found.",
                        Data = default
                    });
                }

                var departmentDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    AgencyId = department.AgencyId ?? 0,
                    LocationId = department.LocationId,
                    AgencyName = department.Agency.Name,
                    LocationName = department.Location.Name,
                    DateCreated = department.DateCreated
                };

                return Ok(new ApiResponse<DepartmentDto>
                {
                    Success = true,
                    Message = "Department retrieved successfully.",
                    Data = departmentDto
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetDepartment", ex);
                return StatusCode(500, new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> CreateDepartment([FromBody] CreateDepartmentDto createDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                               .Select(e => e.ErrorMessage)
                                               .ToList();
                return BadRequest(new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Validation errors occurred.",
                    Errors = errors
                });
            }

            try
            {
                int currentAdminId = GetCurrentAdminId();

                // Validate Location ownership
                var location = await _context.Locations
                    .FirstOrDefaultAsync(l => l.Id == createDepartmentDto.LocationId && l.AdminId == currentAdminId);
                
                if (location == null)
                {
                    return BadRequest(new ApiResponse<DepartmentDto>
                    {
                        Success = false,
                        Message = "Invalid Location ID or you don't have permission to use this location.",
                        Data = null
                    });
                }

                // Validate Agency ownership if provided
                if (createDepartmentDto.AgencyId != 0)
                {
                    var agency = await _context.Agencies
                        .FirstOrDefaultAsync(a => a.Id == createDepartmentDto.AgencyId && a.AdminId == currentAdminId);
                    
                    if (agency == null)
                    {
                        return BadRequest(new ApiResponse<DepartmentDto>
                        {
                            Success = false,
                            Message = "Invalid Agency ID or you don't have permission to use this agency.",
                            Data = null
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

                var departmentDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    AgencyId = department.AgencyId ?? 0,
                    LocationId = department.LocationId,
                    AgencyName = (await _context.Agencies.FindAsync(department.AgencyId))?.Name,
                    LocationName = (await _context.Locations.FindAsync(department.LocationId))?.Name,
                    DateCreated = department.DateCreated
                };

                return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, new ApiResponse<DepartmentDto>
                {
                    Success = true,
                    Message = "Department created successfully.",
                    Data = departmentDto
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in CreateDepartment", ex);
                return StatusCode(500, new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<DepartmentDto>>> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto updateDepartmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Invalid request.",
                    Data = null
                });
            }

            try
            {
                int currentAdminId = GetCurrentAdminId();

                var department = await _context.Departments
                    .Include(d => d.Location)
                    .Include(d => d.Agency)
                    .FirstOrDefaultAsync(d => d.Id == id && 
                        (d.Location.AdminId == currentAdminId || (d.Agency != null && d.Agency.AdminId == currentAdminId)));

                if (department == null)
                {
                    return NotFound(new ApiResponse<DepartmentDto>
                    {
                        Success = false,
                        Message = "Department not found or you don't have permission to update it.",
                        Data = null
                    });
                }

                // Validate new Location ownership
                var location = await _context.Locations
                    .FirstOrDefaultAsync(l => l.Id == updateDepartmentDto.LocationId && l.AdminId == currentAdminId);
                
                if (location == null)
                {
                    return BadRequest(new ApiResponse<DepartmentDto>
                    {
                        Success = false,
                        Message = "Invalid Location ID or you don't have permission to use this location.",
                        Data = null
                    });
                }

                // Validate new Agency ownership if provided
                if (updateDepartmentDto.AgencyId != 0)
                {
                    var agency = await _context.Agencies
                        .FirstOrDefaultAsync(a => a.Id == updateDepartmentDto.AgencyId && a.AdminId == currentAdminId);
                    
                    if (agency == null)
                    {
                        return BadRequest(new ApiResponse<DepartmentDto>
                        {
                            Success = false,
                            Message = "Invalid Agency ID or you don't have permission to use this agency.",
                            Data = null
                        });
                    }
                }

                department.Name = updateDepartmentDto.Name;
                department.Description = updateDepartmentDto.Description;
                department.AgencyId = (int)updateDepartmentDto.AgencyId;
                department.LocationId = (int)updateDepartmentDto.LocationId;

                _context.Entry(department).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var departmentDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    Description = department.Description,
                    AgencyId = department.AgencyId ?? 0,
                    LocationId = department.LocationId,
                    AgencyName = (await _context.Agencies.FindAsync(department.AgencyId))?.Name,
                    LocationName = (await _context.Locations.FindAsync(department.LocationId))?.Name,
                    DateCreated = department.DateCreated
                };

                return Ok(new ApiResponse<DepartmentDto>
                {
                    Success = true,
                    Message = "Department updated successfully.",
                    Data = departmentDto
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in UpdateDepartment", ex);
                return StatusCode(500, new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Internal server error.",
                    Data = null
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteDepartment(int id)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();

                var department = await _context.Departments
                    .Include(d => d.Location)
                    .Include(d => d.Agency)
                    .FirstOrDefaultAsync(d => d.Id == id && 
                        (d.Location.AdminId == currentAdminId || (d.Agency != null && d.Agency.AdminId == currentAdminId)));

                if (department == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Department not found or you don't have permission to delete it.",
                        Data = null
                    });
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Department deleted successfully.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteDepartment", ex);
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
            }
        }
    }
}
