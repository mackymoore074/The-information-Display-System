using System.Net.Http.Json;
using DsplayScreen.IService;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Screen;
using Microsoft.Extensions.Logging;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DsplayScreen.Services
{
    public class ScreenService : IScreenService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ScreenService> _logger;
        private readonly ILocalStorageService _localStorage;

        public ScreenService(
            HttpClient httpClient, 
            ILogger<ScreenService> logger,
            ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _logger = logger;
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<string>> LoginAsync(LoginScreenDto loginDto)
        {
            try
            {
                _logger.LogInformation($"Attempting login for screen: {loginDto.ScreenName}");
                
                var response = await _httpClient.PostAsJsonAsync("api/auth/screen/login", loginDto);
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation($"Response received: {response.StatusCode}");
                _logger.LogInformation($"Response content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
                    if (result.Success)
                    {
                        await _localStorage.SetItemAsStringAsync("authToken", result.Data);
                        _logger.LogInformation("Login successful, token stored");
                    }
                    return result;
                }

                return new ApiResponse<string>
                {
                    Success = false,
                    Message = $"Authentication failed: {response.StatusCode}",
                    Errors = new List<string> { content }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Connection error",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<NewsItem>>> GetNewsItemsAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/screenauth/news-items");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<NewsItem>>>();
                    return result;
                }

                return new ApiResponse<List<NewsItem>>
                {
                    Success = false,
                    Message = $"HTTP Error: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting news items: {ex.Message}");
                return new ApiResponse<List<NewsItem>>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ApiResponse<List<MenuItem>>> GetMenuItemsAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _logger.LogInformation($"Token found: {!string.IsNullOrEmpty(token)}");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No token found in local storage");
                    return new ApiResponse<List<MenuItem>>
                    {
                        Success = false,
                        Message = "No authentication token found"
                    };
                }

                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation("Calling menu-items endpoint...");
                var response = await _httpClient.GetAsync("api/screenauth/menu-items");
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation($"Response Status: {response.StatusCode}");
                _logger.LogInformation($"Response Content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<MenuItem>>>();
                    _logger.LogInformation($"Successfully deserialized {result?.Data?.Count ?? 0} menu items");
                    return result;
                }

                return new ApiResponse<List<MenuItem>>
                {
                    Success = false,
                    Message = $"HTTP Error: {response.StatusCode}",
                    Errors = new List<string> { content }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetMenuItemsAsync: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return new ApiResponse<List<MenuItem>>
                {
                    Success = false,
                    Message = "Error getting menu items",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
} 