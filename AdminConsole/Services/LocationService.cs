using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ClassLibrary.DtoModels.Location;
using ClassLibrary.DtoModels.Common;
using Microsoft.Extensions.Logging;
using AdminConsole.IService;
using Blazored.LocalStorage;

namespace AdminConsole.Services
{
    public class LocationService : ILocationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LocationService> _logger;
        private readonly ILocalStorageService _localStorage;

        public LocationService(HttpClient httpClient, ILogger<LocationService> logger, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _logger = logger;
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<List<LocationDto>>> GetLocationsAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync("api/location");
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<List<LocationDto>>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}",
                        Data = new List<LocationDto>()
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<ApiResponse<List<LocationDto>>>(content, options);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetLocationsAsync: {ex.Message}", ex);
                return new ApiResponse<List<LocationDto>>
                {
                    Success = false,
                    Message = "Error retrieving locations",
                    Data = new List<LocationDto>()
                };
            }
        }

        public async Task<ApiResponse<LocationDto>> GetLocationByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/location/{id}");
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
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("api/location", createLocationDto);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Create response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<LocationDto>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}",
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<ApiResponse<LocationDto>>(content, options);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateLocationAsync: {ex.Message}", ex);
                return new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Error creating location",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<LocationDto>> UpdateLocationAsync(int id, UpdateLocationDto updateLocationDto)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsJsonAsync($"api/location/{id}", updateLocationDto);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Update response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<LocationDto>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}",
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<ApiResponse<LocationDto>>(content, options);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateLocationAsync: {ex.Message}", ex);
                return new ApiResponse<LocationDto>
                {
                    Success = false,
                    Message = "Error updating location",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> DeleteLocationAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/location/{id}");
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
