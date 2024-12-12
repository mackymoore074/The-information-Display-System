using ClassLibrary;
using ClassLibrary.DtoModels.NewsItem;
using ClassLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheWebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsItemController : ControllerBase
    {
        private readonly ClassDBContext _context;

        public NewsItemController(ClassDBContext context)
        {
            _context = context;
        }

        private async Task LogErrorAsync(string message)
        {
            try
            {
                var errorLog = new ErrorLog
                {
                    ErrorMessage = message,
                    DateCreated = DateTime.UtcNow
                };

                await _context.ErrorLogs.AddAsync(errorLog);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Fallback to console logging if database logging fails
                Console.WriteLine($"Failed to log error: {message}");
            }
        }

        // GET: api/newsitems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsItemDto>>> GetNewsItems()
        {
            try
            {
                var newsItems = await _context.NewsItems
                    .Include(n => n.NewsItemAgencies)
                    .Include(n => n.NewsItemScreens)
                    .Include(n => n.NewsItemDepartments)
                    .Include(n => n.NewsItemLocations)
                    .Select(n => new NewsItemDto
                    {
                        Id = n.Id,
                        Title = n.Title,
                        NewsItemBody = n.NewsItemBody,
                        DateCreated = n.DateCreated,
                        LastUpdated = n.LastUpdated,
                        TimeOutDate = n.TimeOutDate,
                        Importance = (NewsItemDto.ImportanceLevelDto)n.Importance,
                        MoreInformationUrl = n.MoreInformationUrl,
                        AgencyId = n.NewsItemAgencies.Select(na => na.AgencyId).ToList(),
                        DepartmentId = n.NewsItemDepartments.Select(nd => nd.DepartmentId).ToList(),
                        LocationId = n.NewsItemLocations.Select(nl => nl.LocationId).ToList(),
                        ScreenId = n.NewsItemScreens.Select(ns => ns.ScreenId).ToList()
                    })
                    .ToListAsync();

                return Ok(newsItems);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex.Message);
                return StatusCode(500, "An error occurred while fetching news items.");
            }
        }

        // GET: api/newsitems/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<NewsItemDto>> GetNewsItem(int id)
        {
            try
            {
                var newsItem = await _context.NewsItems
                    .Include(n => n.NewsItemAgencies)
                    .Include(n => n.NewsItemScreens)
                    .Include(n => n.NewsItemDepartments)
                    .Include(n => n.NewsItemLocations)
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (newsItem == null)
                {
                    return NotFound($"News item with ID {id} not found.");
                }

                return Ok(new NewsItemDto
                {
                    Id = newsItem.Id,
                    Title = newsItem.Title,
                    NewsItemBody = newsItem.NewsItemBody,
                    DateCreated = newsItem.DateCreated,
                    LastUpdated = newsItem.LastUpdated,
                    TimeOutDate = newsItem.TimeOutDate,
                    Importance = (NewsItemDto.ImportanceLevelDto)newsItem.Importance,
                    MoreInformationUrl = newsItem.MoreInformationUrl,
                    AgencyId = newsItem.NewsItemAgencies.Select(na => na.AgencyId).ToList(),
                    DepartmentId = newsItem.NewsItemDepartments.Select(nd => nd.DepartmentId).ToList(),
                    LocationId = newsItem.NewsItemLocations.Select(nl => nl.LocationId).ToList(),
                    ScreenId = newsItem.NewsItemScreens.Select(ns => ns.ScreenId).ToList()
                });
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex.Message);
                return StatusCode(500, "An error occurred while fetching the news item.");
            }
        }

        // POST: api/newsitems
        [HttpPost]
        public async Task<IActionResult> CreateNewsItem([FromBody] CreateNewsItemDto createNewsItemDto)
        {
            try
            {
                // Ensure that AdminId is passed in the request body
                if (createNewsItemDto.AdminId <= 0)
                {
                    return BadRequest("Invalid Admin ID.");
                }

                // Create a new NewsItem object from the DTO
                var newsItem = new NewsItem
                {
                    Title = createNewsItemDto.Title,
                    NewsItemBody = createNewsItemDto.NewsItemBody,
                    TimeOutDate = createNewsItemDto.TimeOutDate,
                    MoreInformationUrl = createNewsItemDto.MoreInformationUrl,
                    AdminId = createNewsItemDto.AdminId  // Use AdminId from the request body
                };

                // Convert string Importance to the enum using Enum.TryParse
                if (Enum.TryParse(createNewsItemDto.Importance.ToString(), out NewsItem.ImportanceLevel importanceLevel))
                {
                    newsItem.Importance = importanceLevel;
                }
                else
                {
                    return BadRequest($"Invalid Importance value: {createNewsItemDto.Importance}");
                }

                // Add the relationships (agencies, departments, etc.)
                foreach (var agencyId in createNewsItemDto.AgencyId)
                {
                    newsItem.NewsItemAgencies.Add(new NewsItemAgency { AgencyId = agencyId });
                }

                foreach (var departmentId in createNewsItemDto.DepartmentId)
                {
                    newsItem.NewsItemDepartments.Add(new NewsItemDepartment { DepartmentId = departmentId });
                }

                foreach (var locationId in createNewsItemDto.LocationId)
                {
                    newsItem.NewsItemLocations.Add(new NewsItemLocation { LocationId = locationId });
                }

                foreach (var screenId in createNewsItemDto.ScreenId)
                {
                    newsItem.NewsItemScreens.Add(new NewsItemScreen { ScreenId = screenId });
                }

                // Save the news item to the database
                _context.NewsItems.Add(newsItem);
                await _context.SaveChangesAsync();

                // Return the created NewsItem DTO
                return Ok(new NewsItemDto
                {
                    Id = newsItem.Id,
                    Title = newsItem.Title,
                    NewsItemBody = newsItem.NewsItemBody,
                    TimeOutDate = newsItem.TimeOutDate,
                    DateCreated = newsItem.DateCreated,
                    LastUpdated = newsItem.LastUpdated,
                    Importance = (NewsItemDto.ImportanceLevelDto)newsItem.Importance,
                    MoreInformationUrl = newsItem.MoreInformationUrl,
                    AgencyId = newsItem.NewsItemAgencies.Select(na => na.AgencyId).ToList(),
                    DepartmentId = newsItem.NewsItemDepartments.Select(nd => nd.DepartmentId).ToList(),
                    LocationId = newsItem.NewsItemLocations.Select(nl => nl.LocationId).ToList(),
                    ScreenId = newsItem.NewsItemScreens.Select(ns => ns.ScreenId).ToList()
                });
            }
            catch (Exception ex)
            {
                // Log the detailed exception message and stack trace for better debugging
                await LogErrorAsync($"Error creating news item: {ex.Message}\n{ex.StackTrace}");
                return StatusCode(500, "An error occurred while creating the news item.");
            }
        }

        // PUT: api/newsitems/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNewsItem(int id, [FromBody] UpdateNewsItemDto updateNewsItemDto)
        {
            try
            {
                var newsItem = await _context.NewsItems
                    .Include(n => n.NewsItemAgencies)
                    .Include(n => n.NewsItemScreens)
                    .Include(n => n.NewsItemDepartments)
                    .Include(n => n.NewsItemLocations)
                    .FirstOrDefaultAsync(n => n.Id == id);

                if (newsItem == null)
                {
                    return NotFound($"News item with ID {id} not found.");
                }

                newsItem.Title = updateNewsItemDto.Title;
                newsItem.NewsItemBody = updateNewsItemDto.NewsItemBody;
                newsItem.TimeOutDate = updateNewsItemDto.TimeOutDate;
                newsItem.Importance = Enum.TryParse<NewsItem.ImportanceLevel>(updateNewsItemDto.Importance.ToString(), out var importanceLevel)
                    ? importanceLevel
                    : throw new ArgumentException("Invalid Importance value");
                newsItem.MoreInformationUrl = updateNewsItemDto.MoreInformationUrl;

                // Clear existing relationships and add new ones
                newsItem.NewsItemAgencies.Clear();
                newsItem.NewsItemDepartments.Clear();
                newsItem.NewsItemLocations.Clear();
                newsItem.NewsItemScreens.Clear();

                foreach (var agencyId in updateNewsItemDto.AgencyId)
                {
                    newsItem.NewsItemAgencies.Add(new NewsItemAgency { AgencyId = agencyId });
                }

                foreach (var departmentId in updateNewsItemDto.DepartmentId)
                {
                    newsItem.NewsItemDepartments.Add(new NewsItemDepartment { DepartmentId = departmentId });
                }

                foreach (var locationId in updateNewsItemDto.LocationId)
                {
                    newsItem.NewsItemLocations.Add(new NewsItemLocation { LocationId = locationId });
                }

                foreach (var screenId in updateNewsItemDto.ScreenId)
                {
                    newsItem.NewsItemScreens.Add(new NewsItemScreen { ScreenId = screenId });
                }

                await _context.SaveChangesAsync();

                return Ok(newsItem);
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex.Message);
                return StatusCode(500, "An error occurred while updating the news item.");
            }
        }

        // DELETE: api/newsitems/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNewsItem(int id)
        {
            try
            {
                var newsItem = await _context.NewsItems.FindAsync(id);
                if (newsItem == null)
                {
                    return NotFound($"News item with ID {id} not found.");
                }

                _context.NewsItems.Remove(newsItem);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await LogErrorAsync(ex.Message);
                return StatusCode(500, "An error occurred while deleting the news item.");
            }
        }
    }
}
