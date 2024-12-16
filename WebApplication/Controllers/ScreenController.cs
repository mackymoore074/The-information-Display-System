using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClassLibrary.Models;
using System.Text.Encodings.Web;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScreenController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<ScreenController> _logger;

        public ScreenController(ClassDBContext context, ILogger<ScreenController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/screen
        [HttpGet("get-screens")]
        public async Task<ActionResult<IEnumerable<ScreenDto>>> GetScreens()
        {
            try
            {
                var screens = await _context.Screens
                    .Include(s => s.Location)
                    .Include(s => s.Agency)
                    .Select(s => new ScreenDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        LocationId = s.LocationId,
                        AgencyId = s.AgencyId,
                        AdminId = s.AdminId,
                        ScreenType = s.ScreenType,
                        LastUpdated = s.LastUpdated,
                        LastCheckedIn = s.LastCheckedIn,
                        IsOnline = s.IsOnline,
                        StatusMessage = s.StatusMessage,
                        MACAddress = s.MACAddress,
                        Location=s.Location,
                        Department=s.Department,
                        Agency=s.Agency,
                        DepartmentId=s.DepartmentId
                    })
                    .ToListAsync();

                return Ok(screens);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetScreens", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: api/screen/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ScreenDto>> GetScreen(int id)
        {
            try
            {
                var screen = await _context.Screens
                    .Include(s => s.Location)
                    .Include(s => s.Agency)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (screen == null)
                    return NotFound($"Screen with ID {id} not found.");

                
                var screenDto = new ScreenDto
                {
                    Id = screen.Id,
                    Name = screen.Name,
                    LocationId = screen.LocationId,
                    AgencyId = screen.AgencyId,
                    AdminId = screen.AdminId,
                    ScreenType = screen.ScreenType,
                    LastUpdated = screen.LastUpdated,
                    LastCheckedIn = screen.LastCheckedIn,
                    IsOnline = screen.IsOnline,
                    StatusMessage = screen.StatusMessage,
                    MACAddress = screen.MACAddress
                };

                return Ok(screenDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in GetScreen", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // POST: api/screen
        [HttpPost]
        public async Task<ActionResult<ScreenDto>> CreateScreen([FromBody] CreateScreenDto createScreenDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var screen = new Screen
                {
                    Name = createScreenDto.Name,
                    LocationId = createScreenDto.LocationId,
                    DepartmentId = createScreenDto.DepartmentId,
                    AgencyId = createScreenDto.AgencyId,
                    AdminId = createScreenDto.AdminId,
                    ScreenType = createScreenDto.ScreenType,
                    LastUpdated = DateTime.UtcNow,
                    LastCheckedIn = DateTime.UtcNow,
                    IsOnline = createScreenDto.IsOnline,
                    StatusMessage = createScreenDto.StatusMessage,
                    MACAddress = createScreenDto.MACAddress,
                    DateCreated = DateTime.UtcNow // Set the current date and time
                };

                _context.Screens.Add(screen);
                await _context.SaveChangesAsync();

                var screenDto = new ScreenDto
                {
                    Id = screen.Id,
                    Name = screen.Name,
                    LocationId = screen.LocationId,
                    AgencyId = screen.AgencyId,
                    AdminId = screen.AdminId,
                    ScreenType = screen.ScreenType,
                    LastUpdated = screen.LastUpdated,
                    LastCheckedIn = screen.LastCheckedIn,
                    IsOnline = screen.IsOnline,
                    StatusMessage = screen.StatusMessage,
                    MACAddress = screen.MACAddress
                };

                return CreatedAtAction(nameof(GetScreen), new { id = screen.Id }, screenDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in CreateScreen", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // PUT: api/screen/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ScreenDto>> UpdateScreen(int id, [FromBody] UpdateScreenDto updateScreenDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var screen = await _context.Screens.FindAsync(id);
                if (screen == null)
                    return NotFound($"Screen with ID {id} not found.");

                screen.Name = updateScreenDto.Name;
                screen.LocationId = updateScreenDto.LocationId;
                screen.AgencyId = updateScreenDto.AgencyId;
                screen.ScreenType = updateScreenDto.ScreenType;
                screen.LastUpdated = DateTime.UtcNow;
                screen.LastCheckedIn = DateTime.UtcNow;
                screen.IsOnline = updateScreenDto.IsOnline;
                screen.StatusMessage = updateScreenDto.StatusMessage;
                screen.MACAddress = updateScreenDto.MACAddress;

                _context.Screens.Update(screen);
                await _context.SaveChangesAsync();

                var screenDto = new ScreenDto
                {
                    Id = screen.Id,
                    Name = screen.Name,
                    LocationId = screen.LocationId,
                    AgencyId = screen.AgencyId,
                    ScreenType = screen.ScreenType,
                    LastUpdated = screen.LastUpdated,
                    LastCheckedIn = screen.LastCheckedIn,
                    IsOnline = screen.IsOnline,
                    StatusMessage = screen.StatusMessage,
                    MACAddress = screen.MACAddress
                };

                return Ok(screenDto);
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in UpdateScreen", ex);
                return StatusCode(500, "Internal server error.");
            }
        }

        // DELETE: api/screen/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScreen(int id)
        {
            try
            {
                var screen = await _context.Screens.FindAsync(id);
                if (screen == null)
                    return NotFound($"Screen with ID {id} not found.");

                _context.Screens.Remove(screen);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await LogErrorToDatabaseAsync("Error in DeleteScreen", ex);
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
