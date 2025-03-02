using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassLibrary.DtoModels.Admin;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using ClassLibrary;
using Microsoft.AspNetCore.Authorization;


namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly IPasswordHasher<Admin> _passwordHasher;

        public AdminController(ClassDBContext context, ILogger<AdminController> logger, IPasswordHasher<Admin> passwordHasher)
        {
            _context = context;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        // GET: api/admin
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminDto>>> GetAdmins()
        {
            try
            {
                var admins = await _context.Admins
                    .Include(a => a.Agency)
                    .Include(a => a.Department)
                    .Include(a => a.Location)
                    .Include(a => a.Screen)
                    .Select(a => new AdminDto
                    {
                        Id = a.Id,
                        FirstName = a.FirstName,
                        LastName = a.LastName,
                        Email = a.Email,
                        Role = a.Role.ToString(),
                        AgencyName = a.Agency.Name,
                        DepartmentName = a.Department.Name,
                        LocationName = a.Location.Name,
                        ScreenName = a.Screen.Name,
                        DateCreated = a.DateCreated,
                        LastLogin = a.LastLogin
                    })
                    .ToListAsync();

                return Ok(admins);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetAdmins", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/admin/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<AdminDto>> GetAdmin(int id)
        {
            try
            {
                var admin = await _context.Admins
                    .Include(a => a.Agency)
                    .Include(a => a.Department)
                    .Include(a => a.Location)
                    .Include(a => a.Screen)
                    .Where(a => a.Id == id)
                    .Select(a => new AdminDto
                    {
                        Id = a.Id,
                        FirstName = a.FirstName,
                        LastName = a.LastName,
                        Email = a.Email,
                        Role = a.Role.ToString(),
                        DateCreated = a.DateCreated,
                        LastLogin = a.LastLogin
                    })
                    .FirstOrDefaultAsync();

                if (admin == null)
                    return NotFound($"Admin with ID {id} not found.");

                return Ok(admin);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetAdmin", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: api/admin
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<AdminDto>> CreateAdmin([FromBody] CreateAdminDto createAdminDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var admin = new Admin
                {
                    FirstName = createAdminDto.FirstName,
                    LastName = createAdminDto.LastName,
                    Email = createAdminDto.Email,
                    Role = createAdminDto.Role,
                    DateCreated = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    PasswordHash = _passwordHasher.HashPassword(null, createAdminDto.PasswordHash)
                };

                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();

                var adminDto = new AdminDto
                {
                    Id = admin.Id,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    Email = admin.Email,
                    Role = admin.Role.ToString(),
                    DateCreated = admin.DateCreated,
                    LastLogin = admin.LastLogin
                };

                return CreatedAtAction(nameof(GetAdmin), new { id = admin.Id }, adminDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in CreateAdmin", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // PUT: api/admin/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<AdminDto>> UpdateAdmin(int id, [FromBody] UpdateAdminDto updateAdminDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null)
                    return NotFound($"Admin with ID {id} not found.");

                admin.FirstName = updateAdminDto.FirstName;
                admin.LastName = updateAdminDto.LastName;
                admin.Email = updateAdminDto.Email;
                admin.Role = updateAdminDto.Role;

                if (!string.IsNullOrEmpty(updateAdminDto.PasswordHash))
                {
                    admin.PasswordHash = _passwordHasher.HashPassword(admin, updateAdminDto.PasswordHash);
                }

                _context.Admins.Update(admin);
                await _context.SaveChangesAsync();

                var adminDto = new AdminDto
                {
                    Id = admin.Id,
                    FirstName = admin.FirstName,
                    LastName = admin.LastName,
                    Email = admin.Email,
                    Role = admin.Role.ToString(),
                    DateCreated = admin.DateCreated,
                    LastLogin = admin.LastLogin
                };

                return Ok(adminDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in UpdateAdmin", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // DELETE: api/admin/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAdmin(int id) //
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null)
                    return NotFound($"Admin with ID {id} not found.");

                _context.Admins.Remove(admin);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteAdmin", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // Add this new endpoint
        [HttpGet("dashboard-analytics")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<DashboardAnalyticsDto>>> GetDashboardAnalytics()
        {
            try
            {
                var analytics = new DashboardAnalyticsDto
                {
                    TotalScreens = await _context.Screens.CountAsync(),
                    ActiveScreens = await _context.Screens
                        .Where(s => _context.DisplayTrackers
                            .Any(dt => dt.ScreenId == s.Id && dt.DisplayedAt >= DateTime.UtcNow.AddMinutes(-15)))
                            .CountAsync(),
                    TotalMenuItems = await _context.MenuItems.CountAsync(),
                    TotalNewsItems = await _context.NewsItems.CountAsync(),

                    TopDisplayedMenuItems = await _context.DisplayTrackers
                        .Where(dt => dt.ItemType == "MenuItem")
                        .GroupBy(dt => new { dt.ItemId })
                        .Select(g => new ItemDisplayStats
                        {
                            ItemId = g.Key.ItemId,
                            Title = _context.MenuItems.FirstOrDefault(m => m.Id == g.Key.ItemId).Title,
                            DisplayCount = g.Count(),
                            LastDisplayed = g.Max(dt => dt.DisplayedAt)
                        })
                        .OrderByDescending(x => x.DisplayCount)
                        .Take(10)
                        .ToListAsync(),

                    TopDisplayedNewsItems = await _context.DisplayTrackers
                        .Where(dt => dt.ItemType == "NewsItem")
                        .GroupBy(dt => new { dt.ItemId })
                        .Select(g => new ItemDisplayStats
                        {
                            ItemId = g.Key.ItemId,
                            Title = _context.NewsItems.FirstOrDefault(n => n.Id == g.Key.ItemId).Title,
                            DisplayCount = g.Count(),
                            LastDisplayed = g.Max(dt => dt.DisplayedAt)
                        })
                        .OrderByDescending(x => x.DisplayCount)
                        .Take(10)
                        .ToListAsync(),

                    ScreenActivities = await _context.Screens
                        .Include(s => s.Location)
                        .Select(s => new ScreenActivityStats
                        {
                            ScreenId = s.Id,
                            ScreenName = s.Name ?? "Unknown",
                            Location = s.Location != null ? s.Location.Name ?? "Unknown" : "Unknown",
                            TotalDisplays = _context.DisplayTrackers.Count(dt => dt.ScreenId == s.Id),
                            LastActive = _context.DisplayTrackers
                                .Where(dt => dt.ScreenId == s.Id)
                                .OrderByDescending(dt => dt.DisplayedAt)
                                .Select(dt => dt.DisplayedAt)
                                .FirstOrDefault(),
                            IsCurrentlyActive = _context.DisplayTrackers
                                .Any(dt => dt.ScreenId == s.Id && dt.DisplayedAt >= DateTime.UtcNow.AddMinutes(-15))
                        })
                        .ToListAsync()
                };

                // Initialize empty lists if null
                analytics.ScreenActivities ??= new List<ScreenActivityStats>();
                analytics.TopDisplayedMenuItems ??= new List<ItemDisplayStats>();
                analytics.TopDisplayedNewsItems ??= new List<ItemDisplayStats>();

                return Ok(new ApiResponse<DashboardAnalyticsDto>
                {
                    Success = true,
                    Data = analytics,
                    Message = "Dashboard analytics retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetDashboardAnalytics", ex);
                return StatusCode(500, new ApiResponse<DashboardAnalyticsDto>
                {
                    Success = false,
                    Message = "Error retrieving dashboard analytics"
                });
            }
        }

        // Helper method to log errors to the database
        private async Task LogErrorToDatabaseAsync(string context, Exception ex)
        {
            _logger.LogError($"{context}: {ex.Message}", ex);

            // Ensure error message can be logged even if database access fails
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
