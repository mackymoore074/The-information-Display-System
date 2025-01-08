using AdminConsole.IService;
using ClassLibrary.DtoModels.Admin;
using ClassLibrary.DtoModels.Common;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AdminConsole.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(HttpClient httpClient, ILocalStorageService localStorage, ILogger<AnalyticsService> logger)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _logger = logger;
        }

        public async Task<ApiResponse<DashboardAnalyticsDto>> GetDashboardAnalyticsAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/admin/dashboard-analytics");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ApiResponse<DashboardAnalyticsDto>>();
                }

                return new ApiResponse<DashboardAnalyticsDto>
                {
                    Success = false,
                    Message = $"Error: {response.StatusCode}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting dashboard analytics: {ex.Message}");
                return new ApiResponse<DashboardAnalyticsDto>
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
} 