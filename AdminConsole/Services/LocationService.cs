using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClassLibrary;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Location;
using ClassLibrary.DtoModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AdminConsole.IService;

namespace TheWebApplication.Services
{
    public class LocationService : ILocationService
    {
        private readonly ClassDBContext _context;
        private readonly ILogger<LocationService> _logger;

        public LocationService(ClassDBContext context, ILogger<LocationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<List<LocationDto>>> GetLocationsAsync()
        {
            try
            {
                var locations = await _context.Locations
                    .Select(l => new LocationDto
                    {
                        Id = l.Id,
                        Name = l.Name,
                        Address = l.Address,
                        DateCreated = l.DateCreated
                    })
                    .ToListAsync(); // This is now explicitly returning List<LocationDto>

                return new ApiResponse<List<LocationDto>>
                {
                    Success = true,
                    Message = "Locations retrieved successfully",
                    Data = locations
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetLocationsAsync: {ex.Message}", ex);
                return new ApiResponse<List<LocationDto>>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                };
            }
        }



        public async Task<ApiResponse<LocationDto>> GetLocationByIdAsync(int id)
        {
            try
            {
                var location = await _context.Locations
                    .Include(l => l.Departments)
                    .Include(l => l.Screens)
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (location == null)
                {
                    return new ApiResponse<LocationDto>
                    {
                        Success = false,
                        Message = "Location not found",
                        Errors = new List<string> { $"Location with ID {id} not found" }
                    };
                }

                var locationDto = new LocationDto
                {
                    Id = location.Id,
                    Name = location.Name,
                    Address = location.Address,
                    DateCreated = location.DateCreated
                };

                return new ApiResponse<LocationDto>
                {
                    Success = true,
                    Message = "Location retrieved successfully",
                    Data = locationDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetLocationByIdAsync: {ex.Message}", ex);
                return new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                };
            }
        }

        public async Task<ApiResponse<LocationDto>> CreateLocationAsync(CreateLocationDto createLocationDto)
        {
            try
            {
                var location = new Location
                {
                    Name = createLocationDto.Name,
                    Address = createLocationDto.Address,
                    DateCreated = DateTime.UtcNow
                };

                _context.Locations.Add(location);
                await _context.SaveChangesAsync();

                var locationDto = new LocationDto
                {
                    Id = location.Id,
                    Name = location.Name,
                    Address = location.Address,
                    DateCreated = location.DateCreated
                };

                return new ApiResponse<LocationDto>
                {
                    Success = true,
                    Message = "Location created successfully",
                    Data = locationDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateLocationAsync: {ex.Message}", ex);
                return new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                };
            }
        }

        public async Task<ApiResponse<LocationDto>> UpdateLocationAsync(int id, UpdateLocationDto updateLocationDto)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                {
                    return new ApiResponse<LocationDto>
                    {
                        Success = false,
                        Message = "Location not found",
                        Errors = new List<string> { $"Location with ID {id} not found" }
                    };
                }

                location.Name = updateLocationDto.Name;
                location.Address = updateLocationDto.Address;

                await _context.SaveChangesAsync();

                var locationDto = new LocationDto
                {
                    Id = location.Id,
                    Name = location.Name,
                    Address = location.Address,
                    DateCreated = location.DateCreated
                };

                return new ApiResponse<LocationDto>
                {
                    Success = true,
                    Message = "Location updated successfully",
                    Data = locationDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateLocationAsync: {ex.Message}", ex);
                return new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                };
            }
        }

        public async Task<ApiResponse<object>> DeleteLocationAsync(int id)
        {
            try
            {
                var location = await _context.Locations.FindAsync(id);
                if (location == null)
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Location not found",
                        Errors = new List<string> { $"Location with ID {id} not found" }
                    };
                }

                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "Location deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteLocationAsync: {ex.Message}", ex);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Internal server error",
                    Errors = new List<string> { "An unexpected error occurred" }
                };
            }
        }
    }
}
