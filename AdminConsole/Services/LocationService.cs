using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ClassLibrary.DtoModels.Location;
using ClassLibrary.DtoModels.Common;
using Microsoft.Extensions.Logging;
using AdminConsole.IService;

namespace AdminConsole.Services
{
    public class LocationService : ILocationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LocationService> _logger;

        public LocationService(HttpClient httpClient, ILogger<LocationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<ApiResponse<List<LocationDto>>> GetLocationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/locations");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var locations = JsonSerializer.Deserialize<ApiResponse<List<LocationDto>>>(content);
                return locations;
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
                var response = await _httpClient.GetAsync($"api/locations/{id}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var location = JsonSerializer.Deserialize<ApiResponse<LocationDto>>(content);
                return location;
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
                var response = await _httpClient.PostAsJsonAsync("api/locations", createLocationDto);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var location = JsonSerializer.Deserialize<ApiResponse<LocationDto>>(content);
                return location;
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
                var response = await _httpClient.PutAsJsonAsync($"api/locations/{id}", updateLocationDto);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var location = JsonSerializer.Deserialize<ApiResponse<LocationDto>>(content);
                return location;
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
                var response = await _httpClient.DeleteAsync($"api/locations/{id}");
                response.EnsureSuccessStatusCode();
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
