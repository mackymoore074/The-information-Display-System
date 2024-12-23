using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.NewsItem;
using ClassLibrary.DtoModels.Common;
using Microsoft.Extensions.Logging;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NewsItemController : ControllerBase
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<NewsItemController> _logger;

        public NewsItemController(ClassDBContext context, ILogger<NewsItemController> logger)
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

        [HttpPost]
        public async Task<ActionResult<ApiResponse<NewsItem>>> Create([FromBody] CreateNewsItemDto createDto)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();

                if (createDto.TimeOutDate <= DateTime.UtcNow)
                {
                    return BadRequest(new ApiResponse<NewsItem>
                    {
                        Success = false,
                        Message = "TimeOutDate must be in the future",
                        Errors = new List<string> { "Invalid TimeOutDate" }
                    });
                }

                var newsItem = new NewsItem
                {
                    Title = createDto.Title,
                    NewsItemBody = createDto.NewsItemBody,
                    TimeOutDate = createDto.TimeOutDate,
                    Importance = createDto.Importance,
                    MoreInformationUrl = createDto.MoreInformationUrl,
                    AdminId = currentAdminId,
                    Departments = createDto.Departments,
                    Screens = createDto.Screens,
                    Locations = createDto.Locations
                };

                await _context.NewsItems.AddAsync(newsItem);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<NewsItem>
                {
                    Success = true,
                    Message = "News item created successfully",
                    Data = newsItem
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating news item: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<NewsItem>
                {
                    Success = false,
                    Message = "Error creating news item",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<NewsItem>>>> GetAll()
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();
                var newsItems = await _context.NewsItems
                    .Where(n => n.AdminId == currentAdminId)
                    .OrderByDescending(n => n.DateCreated)
                    .ToListAsync();

                return Ok(new ApiResponse<List<NewsItem>>
                {
                    Success = true,
                    Data = newsItems
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting news items: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<List<NewsItem>>
                {
                    Success = false,
                    Message = "Error retrieving news items",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<NewsItem>>> GetById(int id)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();
                var newsItem = await _context.NewsItems
                    .FirstOrDefaultAsync(n => n.Id == id && n.AdminId == currentAdminId);

                if (newsItem == null)
                {
                    return NotFound(new ApiResponse<NewsItem>
                    {
                        Success = false,
                        Message = "News item not found"
                    });
                }

                return Ok(new ApiResponse<NewsItem>
                {
                    Success = true,
                    Data = newsItem
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting news item: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<NewsItem>
                {
                    Success = false,
                    Message = "Error retrieving news item",
                    Errors = new List<string> { ex.Message }
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<NewsItem>>> Update(int id, [FromBody] CreateNewsItemDto updateDto)
        {
            try
            {
                int currentAdminId = GetCurrentAdminId();
                var newsItem = await _context.NewsItems
                    .FirstOrDefaultAsync(n => n.Id == id && n.AdminId == currentAdminId);

                if (newsItem == null)
                {
                    return NotFound(new ApiResponse<NewsItem>
                    {
                        Success = false,
                        Message = "News item not found"
                    });
                }

                if (updateDto.TimeOutDate <= DateTime.UtcNow)
                {
                    return BadRequest(new ApiResponse<NewsItem>
                    {
                        Success = false,
                        Message = "TimeOutDate must be in the future",
                        Errors = new List<string> { "Invalid TimeOutDate" }
                    });
                }

                newsItem.Title = updateDto.Title;
                newsItem.NewsItemBody = updateDto.NewsItemBody;
                newsItem.TimeOutDate = updateDto.TimeOutDate;
                newsItem.Importance = updateDto.Importance;
                newsItem.MoreInformationUrl = updateDto.MoreInformationUrl;
                newsItem.LastUpdated = DateTime.UtcNow;
                newsItem.Departments = updateDto.Departments;
                newsItem.Screens = updateDto.Screens;
                newsItem.Locations = updateDto.Locations;

                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<NewsItem>
                {
                    Success = true,
                    Message = "News item updated successfully",
                    Data = newsItem
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating news item: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<NewsItem>
                {
                    Success = false,
                    Message = "Error updating news item",
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
                var newsItem = await _context.NewsItems
                    .FirstOrDefaultAsync(n => n.Id == id && n.AdminId == currentAdminId);

                if (newsItem == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "News item not found"
                    });
                }

                _context.NewsItems.Remove(newsItem);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "News item deleted successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting news item: {ex.Message}", ex);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting news item",
                    Errors = new List<string> { ex.Message }
                });
            }
        }
    }
} 