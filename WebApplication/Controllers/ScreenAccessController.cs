using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScreenAccessController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<ScreenAccessController> _logger;

        public ScreenAccessController(ClassDBContext context, ILogger<ScreenAccessController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("track/{screenId}")]
        public async Task<ActionResult<ApiResponse<bool>>> TrackScreenAccess(int screenId)
        {
            try
            {
                var screenAccess = new ScreenAccess
                {
                    ScreenId = screenId,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                    LastAccessTime = DateTime.UtcNow,
                    IsActive = true
                };

                _context.ScreenAccesses.Add(screenAccess);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Message = "Screen access tracked successfully",
                    Data = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error tracking screen access: {ex.Message}");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Error tracking screen access",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("current")]
        public async Task<ActionResult<ApiResponse<Screen>>> GetCurrentScreen()
        {
            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                
                var screen = await _context.ScreenAccesses
                    .Where(sa => sa.IpAddress == ipAddress && sa.IsActive)
                    .OrderByDescending(sa => sa.LastAccessTime)
                    .Select(sa => sa.Screen)
                    .FirstOrDefaultAsync();

                if (screen == null)
                    return NotFound(new ApiResponse<Screen>
                    {
                        Success = false,
                        Message = "No screen found for current IP address"
                    });

                return Ok(new ApiResponse<Screen>
                {
                    Success = true,
                    Message = "Current screen retrieved successfully",
                    Data = screen
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting current screen: {ex.Message}");
                return StatusCode(500, new ApiResponse<Screen>
                {
                    Success = false,
                    Message = "Error retrieving current screen",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("history/{screenId}")]
        public async Task<ActionResult<ApiResponse<List<ScreenAccess>>>> GetScreenAccessHistory(int screenId)
        {
            try
            {
                var accessHistory = await _context.ScreenAccesses
                    .Where(sa => sa.ScreenId == screenId)
                    .OrderByDescending(sa => sa.LastAccessTime)
                    .Take(5)
                    .ToListAsync();

                return Ok(new ApiResponse<List<ScreenAccess>>
                {
                    Success = true,
                    Message = "Screen access history retrieved successfully",
                    Data = accessHistory
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving screen access history: {ex.Message}");
                return StatusCode(500, new ApiResponse<List<ScreenAccess>>
                {
                    Success = false,
                    Message = "Error retrieving screen access history"
                });
            }
        }
    }
} 