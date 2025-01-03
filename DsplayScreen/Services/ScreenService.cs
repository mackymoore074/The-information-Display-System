using System.Net.Http.Json;
using DsplayScreen.IService;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Screen;
using Microsoft.Extensions.Logging;
using Blazored.LocalStorage;
using System.Net.Http.Headers;

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
                var response = await _httpClient.PostAsJsonAsync("api/Screen/login", loginDto);
                var content = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Error during login",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<NewsItem>>> GetNewsItemsAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("screenToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/Screen/news-items");
                var content = await response.Content.ReadFromJsonAsync<ApiResponse<List<NewsItem>>>();
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting news items: {ex.Message}");
                return new ApiResponse<List<NewsItem>>
                {
                    Success = false,
                    Message = "Error getting news items",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<List<MenuItem>>> GetMenuItemsAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("screenToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/Screen/menu-items");
                var content = await response.Content.ReadFromJsonAsync<ApiResponse<List<MenuItem>>>();
                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting menu items: {ex.Message}");
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