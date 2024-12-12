using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassLibrary.DtoModels.MenuItem;
using ClassLibrary.Models;
using ClassLibrary;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuItemsController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<MenuItemsController> _logger;

        public MenuItemsController(ClassDBContext context, ILogger<MenuItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/menuitems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuItems()
        {
            try
            {
                var menuItems = await _context.MenuItems
                    .Include(m => m.Admin)
                    .Include(m => m.Agency)
                    .Select(m => new MenuItemDto
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Description = m.Description,
                        StartDate = m.StartDate,
                        EndDate = m.EndDate,
                        IsExpired = m.IsExpired,
                        AdminId = m.AdminId,
                        AgencyId = m.AgencyId
                    })
                    .ToListAsync();

                return Ok(menuItems);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetMenuItems", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/menuitems/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItemDto>> GetMenuItem(int id)
        {
            try
            {
                var menuItem = await _context.MenuItems
                    .Include(m => m.Admin)
                    .Include(m => m.Agency)
                    .Where(m => m.Id == id)
                    .Select(m => new MenuItemDto
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Description = m.Description,
                        StartDate = m.StartDate,
                        EndDate = m.EndDate,
                        IsExpired = m.IsExpired,
                        AdminId = m.AdminId,
                        AgencyId = m.AgencyId
                    })
                    .FirstOrDefaultAsync();

                if (menuItem == null)
                    return NotFound($"MenuItem with ID {id} not found.");

                return Ok(menuItem);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetMenuItem", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: api/menuitems
        [HttpPost]
        public async Task<ActionResult<MenuItemDto>> CreateMenuItem([FromBody] CreateMenuItemDto createMenuItemDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var menuItem = new MenuItems
                {
                    Title = createMenuItemDto.Title,
                    Description = createMenuItemDto.Description,
                    StartDate = createMenuItemDto.StartDate,
                    EndDate = createMenuItemDto.EndDate,
                    IsExpired = createMenuItemDto.IsExpired,
                    AdminId = createMenuItemDto.AdminId,
                    AgencyId = createMenuItemDto.AgencyId
                };

                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();

                var menuItemDto = new MenuItemDto
                {
                    Id = menuItem.Id,
                    Title = menuItem.Title,
                    Description = menuItem.Description,
                    StartDate = menuItem.StartDate,
                    EndDate = menuItem.EndDate,
                    IsExpired = menuItem.IsExpired,
                    AdminId = menuItem.AdminId,
                    AgencyId = menuItem.AgencyId
                };

                return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItemDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in CreateMenuItem", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // PUT: api/menuitems/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<MenuItemDto>> UpdateMenuItem(int id, [FromBody] CreateMenuItemDto updateMenuItemDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var menuItem = await _context.MenuItems.FindAsync(id);
                if (menuItem == null)
                    return NotFound($"MenuItem with ID {id} not found.");

                menuItem.Title = updateMenuItemDto.Title;
                menuItem.Description = updateMenuItemDto.Description;
                menuItem.StartDate = updateMenuItemDto.StartDate;
                menuItem.EndDate = updateMenuItemDto.EndDate;
                menuItem.IsExpired = updateMenuItemDto.IsExpired;
                menuItem.AdminId = updateMenuItemDto.AdminId;
                menuItem.AgencyId = updateMenuItemDto.AgencyId;

                _context.MenuItems.Update(menuItem);
                await _context.SaveChangesAsync();

                var menuItemDto = new MenuItemDto
                {
                    Id = menuItem.Id,
                    Title = menuItem.Title,
                    Description = menuItem.Description,
                    StartDate = menuItem.StartDate,
                    EndDate = menuItem.EndDate,
                    IsExpired = menuItem.IsExpired,
                    AdminId = menuItem.AdminId,
                    AgencyId = menuItem.AgencyId
                };

                return Ok(menuItemDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in UpdateMenuItem", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // DELETE: api/menuitems/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            try
            {
                var menuItem = await _context.MenuItems.FindAsync(id);
                if (menuItem == null)
                    return NotFound($"MenuItem with ID {id} not found.");

                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteMenuItem", ex);
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
