using System.Net.Http.Json;
using AdminConsole.IService;
using Blazored.LocalStorage;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.NewsItem;
using ClassLibrary.DtoModels.Common;
using Microsoft.Extensions.Logging;

namespace AdminConsole.Services
{
    public class NewsItemService : INewsItemService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NewsItemService> _logger;
        private readonly ILocalStorageService _localStorage;

        public NewsItemService(HttpClient httpClient, ILogger<NewsItemService> logger, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _logger = logger;
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<List<NewsItem>>> GetAllNewsItemsAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                return await _httpClient.GetFromJsonAsync<ApiResponse<List<NewsItem>>>("api/NewsItem");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting news items");
                throw;
            }
        }

        public async Task<ApiResponse<NewsItem>> GetNewsItemByIdAsync(int id)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                return await _httpClient.GetFromJsonAsync<ApiResponse<NewsItem>>($"api/NewsItem/{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting news item");
                throw;
            }
        }

        public async Task<ApiResponse<NewsItem>> CreateNewsItemAsync(CreateNewsItemDto newsItem)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.PostAsJsonAsync("api/NewsItem", newsItem);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<NewsItem>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating news item");
                throw;
            }
        }

        public async Task<ApiResponse<NewsItem>> UpdateNewsItemAsync(int id, CreateNewsItemDto newsItem)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.PutAsJsonAsync($"api/NewsItem/{id}", newsItem);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<NewsItem>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating news item");
                throw;
            }
        }

        public async Task<ApiResponse<object>> DeleteNewsItemAsync(int id)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.DeleteAsync($"api/NewsItem/{id}");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting news item");
                throw;
            }
        }
    }
} 