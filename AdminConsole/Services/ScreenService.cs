using AdminConsole.IService;
using Blazored.LocalStorage;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Screen;
using System.Net.Http.Json;
using System.Text.Json;

namespace AdminConsole.Services
{
    public class ScreenService : IScreenService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ScreenService> _logger;
        private readonly ILocalStorageService _localStorage;

        public ScreenService(HttpClient httpClient, ILogger<ScreenService> logger, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _logger = logger;
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<List<ScreenDto>>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Getting all screens");
                var response = await _httpClient.GetAsync("api/screen");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ScreenDto>>>();
                    return result ?? new ApiResponse<List<ScreenDto>> 
                    { 
                        Success = false, 
                        Message = "Failed to deserialize response",
                        Data = new List<ScreenDto>() 
                    };
                }
                
                return new ApiResponse<List<ScreenDto>>
                {
                    Success = false,
                    Message = $"Error: {response.StatusCode} - {response.ReasonPhrase}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting screens");
                return new ApiResponse<List<ScreenDto>> 
                { 
                    Success = false, 
                    Message = ex.Message,
                    Data = new List<ScreenDto>() 
                };
            }
        }

        public async Task<ApiResponse<ScreenDto>> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetFromJsonAsync<ApiResponse<ScreenDto>>($"/api/screen/{id}");
                return response ?? new ApiResponse<ScreenDto> { Success = false, Message = "Failed to get screen" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<ScreenDto> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ApiResponse<ScreenDto>> CreateAsync(CreateScreenDto createScreenDto)
        {
            try
            {
                _logger.LogInformation("Creating new screen");
                var response = await _httpClient.PostAsJsonAsync("api/screen", createScreenDto);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Create response content: {content}");

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<ScreenDto>>(content, options);
                    return result ?? new ApiResponse<ScreenDto>
                    {
                        Success = false,
                        Message = "Failed to deserialize response"
                    };
                }

                // For error responses, create a new ApiResponse
                return new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = $"Error: {response.StatusCode}",
                    Errors = new List<string> { content }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating screen");
                return new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Error creating screen",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<ScreenDto>> UpdateAsync(int id, UpdateScreenDto updateScreenDto)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsJsonAsync($"api/screen/{id}", updateScreenDto);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Update response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<ScreenDto>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}",
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<ApiResponse<ScreenDto>>(content, options);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateAsync: {ex.Message}", ex);
                return new ApiResponse<ScreenDto>
                {
                    Success = false,
                    Message = "Error updating screen",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/screen/{id}");
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>()
                    ?? new ApiResponse<bool> { Success = false, Message = "Failed to delete screen" };
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool> { Success = false, Message = ex.Message };
            }
        }
    }
} 