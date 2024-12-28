using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Email;
using ClassLibrary.DtoModels.MenuItem;
using ClassLibrary.DtoModels.Common;
using Microsoft.Extensions.Logging;


namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MenuItemController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<MenuItemController> _logger;
        private readonly IEmailRepository _emailRepository;

        public MenuItemController(
            ClassDBContext context, 
            ILogger<MenuItemController> logger,
            IEmailRepository emailRepository)
        {
            _context = context;
            _logger = logger;
            _emailRepository = emailRepository;
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

        [HttpPost]
        public async Task<ActionResult<ApiResponse<MenuItem>>> Create([FromBody] CreateMenuItemDto createDto)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();

                if (createDto.TimeOutDate <= DateTime.UtcNow)
                {
                    return BadRequest(new ApiResponse<MenuItem>
                    {
                        Success = false,
                        Message = "TimeOutDate must be in the future",
                        Errors = new List<string> { "Invalid TimeOutDate" }
                    });
                }

                var menuItem = new MenuItem
                {
                    Title = createDto.Title,
                    Description = createDto.Description,
                    TimeOutDate = createDto.TimeOutDate,
                    Type = createDto.Type,
                    Price = createDto.Price,
                    AdminId = currentAdminId,
                    Departments = createDto.Departments,
                    Screens = createDto.Screens,
                    Locations = createDto.Locations
                };

                await _context.MenuItems.AddAsync(menuItem);
                await _context.SaveChangesAsync();

                var emailDto = new EmailDto
                {
                    Subject = $"New Menu Item: {createDto.Title}",
                    Body = $@"
                        A new menu item has been posted:
                        Title: {createDto.Title}
                        Description: {createDto.Description}
                        Price: {createDto.Price:C}
                        Type: {createDto.Type}
                        Time Out Date: {createDto.TimeOutDate:g}
                    "
                };

                await _emailRepository.SendEmailToStaffAsync(emailDto);

                return Ok(new ApiResponse<MenuItem>
                {
                    Success = true,
                    Message = "Menu item created successfully",
                    Data = menuItem
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating menu item: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<MenuItem>
                {
                    Success = false,
                    Message = "Error creating menu item",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<MenuItem>>>> GetAll()
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();
                var menuItems = await _context.MenuItems
                    .Where(m => m.AdminId == currentAdminId)
                    .OrderByDescending(m => m.DateCreated)
                    .ToListAsync();

                return Ok(new ApiResponse<List<MenuItem>>
                {
                    Success = true,
                    Data = menuItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting menu items: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<List<MenuItem>>
                {
                    Success = false,
                    Message = "Error retrieving menu items",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MenuItem>>> GetById(int id)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();
                var menuItem = await _context.MenuItems
                    .FirstOrDefaultAsync(m => m.Id == id && m.AdminId == currentAdminId);

                if (menuItem == null)
                {
                    return NotFound(new ApiResponse<MenuItem>
                    {
                        Success = false,
                        Message = "Menu item not found"
                    });
                }

                return Ok(new ApiResponse<MenuItem>
                {
                    Success = true,
                    Data = menuItem
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting menu item: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<MenuItem>
                {
                    Success = false,
                    Message = "Error retrieving menu item",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<MenuItem>>> Update(int id, [FromBody] CreateMenuItemDto updateDto)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();
                var menuItem = await _context.MenuItems
                    .FirstOrDefaultAsync(m => m.Id == id && m.AdminId == currentAdminId);

                if (menuItem == null)
                {
                    return NotFound(new ApiResponse<MenuItem>
                    {
                        Success = false,
                        Message = "Menu item not found"
                    });
                }

                if (updateDto.TimeOutDate <= DateTime.UtcNow)
                {
                    return BadRequest(new ApiResponse<MenuItem>
                    {
                        Success = false,
                        Message = "TimeOutDate must be in the future",
                        Errors = new List<string> { "Invalid TimeOutDate" }
                    });
                }

                menuItem.Title = updateDto.Title;
                menuItem.Description = updateDto.Description;
                menuItem.TimeOutDate = updateDto.TimeOutDate;
                menuItem.Type = updateDto.Type;
                menuItem.Price = updateDto.Price;
                menuItem.LastUpdated = DateTime.UtcNow;
                menuItem.Departments = updateDto.Departments;
                menuItem.Screens = updateDto.Screens;
                menuItem.Locations = updateDto.Locations;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<MenuItem>
                {
                    Success = true,
                    Message = "Menu item updated successfully",
                    Data = menuItem
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating menu item: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<MenuItem>
                {
                    Success = false,
                    Message = "Error updating menu item",
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
                var menuItem = await _context.MenuItems
                    .FirstOrDefaultAsync(m => m.Id == id && m.AdminId == currentAdminId);

                if (menuItem == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Menu item not found"
                    });
                }

                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Menu item deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting menu item: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting menu item",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
} 