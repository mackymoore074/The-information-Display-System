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
    public class PublicDisplayService : IPublicDisplayService
    {
        private readonly HttpClient _httpClient;

        public PublicDisplayService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<ScreenDto>> GetScreenByIpAsync()
        {
            return await _httpClient.GetFromJsonAsync<ApiResponse<ScreenDto>>("api/screen/by-ip");
        }

        public async Task<ApiResponse<List<MenuItem>>> GetMenuItemsForScreenAsync(int screenId)
        {
            return await _httpClient.GetFromJsonAsync<ApiResponse<List<MenuItem>>>($"api/screen/{screenId}/menu-items");
        }

        public async Task<ApiResponse<List<NewsItem>>> GetNewsItemsForScreenAsync(int screenId)
        {
            return await _httpClient.GetFromJsonAsync<ApiResponse<List<NewsItem>>>($"api/screen/{screenId}/news-items");
        }

        public async Task<ApiResponse<bool>> TrackDisplaysAsync(int screenId, List<DisplayTracker> displays)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/screen/{screenId}/track-displays", displays);
            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
    } 
}