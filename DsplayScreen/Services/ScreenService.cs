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

                // Try to get fresh data from backend
                var response = await _httpClient.GetAsync("api/screenauth/news-items");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<NewsItem>>>();
                    if (result.Success && result.Data != null)
                    {
                        // Store fresh data in localStorage
                        await _localStorage.SetItemAsync("cached_news_items", result.Data);
                        await _localStorage.SetItemAsync("news_items_last_updated", DateTime.Now);
                        return result;
                    }
                }

                // If backend call fails, try to get cached data
                var cachedData = await _localStorage.GetItemAsync<List<NewsItem>>("cached_news_items");
                var lastUpdated = await _localStorage.GetItemAsync<DateTime>("news_items_last_updated");

                if (cachedData != null)
                {
                    _logger.LogInformation($"Using cached news items from {lastUpdated}");
                    return new ApiResponse<List<NewsItem>>
                    {
                        Success = true,
                        Data = cachedData,
                        Message = $"Using cached data from {lastUpdated}"
                    };
                }

                return new ApiResponse<List<NewsItem>>
                {
                    Success = false,
                    Message = "No data available"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetNewsItemsAsync: {ex.Message}");
                
                // Try to get cached data in case of any error
                var cachedData = await _localStorage.GetItemAsync<List<NewsItem>>("cached_news_items");
                var lastUpdated = await _localStorage.GetItemAsync<DateTime>("news_items_last_updated");

                if (cachedData != null)
                {
                    _logger.LogInformation($"Using cached news items from {lastUpdated}");
                    return new ApiResponse<List<NewsItem>>
                    {
                        Success = true,
                        Data = cachedData,
                        Message = $"Using cached data from {lastUpdated}"
                    };
                }

                return new ApiResponse<List<NewsItem>>
                {
                    Success = false,
                    Message = "Error getting news items and no cached data available",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<MenuItem>>> GetMenuItemsAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);

                // Try to get fresh data from backend
                var response = await _httpClient.GetAsync("api/screenauth/menu-items");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<MenuItem>>>();
                    if (result.Success && result.Data != null)
                    {
                        // Store fresh data in localStorage
                        await _localStorage.SetItemAsync("cached_menu_items", result.Data);
                        await _localStorage.SetItemAsync("menu_items_last_updated", DateTime.Now);
                        return result;
                    }
                }

                // If backend call fails, try to get cached data
                var cachedData = await _localStorage.GetItemAsync<List<MenuItem>>("cached_menu_items");
                var lastUpdated = await _localStorage.GetItemAsync<DateTime>("menu_items_last_updated");

                if (cachedData != null)
                {
                    _logger.LogInformation($"Using cached menu items from {lastUpdated}");
                    return new ApiResponse<List<MenuItem>>
                    {
                        Success = true,
                        Data = cachedData,
                        Message = $"Using cached data from {lastUpdated}"
                    };
                }

                return new ApiResponse<List<MenuItem>>
                {
                    Success = false,
                    Message = "No data available"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetMenuItemsAsync: {ex.Message}");
                
                // Try to get cached data in case of any error
                var cachedData = await _localStorage.GetItemAsync<List<MenuItem>>("cached_menu_items");
                var lastUpdated = await _localStorage.GetItemAsync<DateTime>("menu_items_last_updated");

                if (cachedData != null)
                {
                    _logger.LogInformation($"Using cached menu items from {lastUpdated}");
                    return new ApiResponse<List<MenuItem>>
                    {
                        Success = true,
                        Data = cachedData,
                        Message = $"Using cached data from {lastUpdated}"
                    };
                }

                return new ApiResponse<List<MenuItem>>
                {
                    Success = false,
                    Message = "Error getting menu items and no cached data available",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
} 