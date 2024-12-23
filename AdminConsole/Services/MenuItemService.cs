using AdminConsole.IService;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.MenuItem;
using Blazored.LocalStorage;
using System.Net.Http.Json;

namespace AdminConsole.Services
{
    public class MenuItemService : IMenuItemService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MenuItemService> _logger;
        private readonly ILocalStorageService _localStorage;

        public MenuItemService(HttpClient httpClient, 
            ILogger<MenuItemService> logger,
            ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _logger = logger;
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<List<MenuItem>>> GetAllMenuItemsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all menu items");
                
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/MenuItem");
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"MenuItem response content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<MenuItem>>>();
                    return result ?? new ApiResponse<List<MenuItem>>
                    {
                        Success = false,
                        Message = "Failed to deserialize menu items response"
                    };
                }
                else
                {
                    _logger.LogWarning($"Failed to get menu items. Status: {response.StatusCode}");
                    return new ApiResponse<List<MenuItem>>
                    {
                        Success = false,
                        Message = $"Failed to get menu items. Status: {response.StatusCode}",
                        Errors = new List<string> { content }
                    };
                }
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

        public async Task<ApiResponse<MenuItem>> CreateMenuItemAsync(CreateMenuItemDto menuItem)
        {
            try
            {
                _logger.LogInformation($"Creating menu item: {System.Text.Json.JsonSerializer.Serialize(menuItem)}");
                
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("api/MenuItem", menuItem);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Create menu item response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<MenuItem>>();
                    return result ?? new ApiResponse<MenuItem>
                    {
                        Success = false,
                        Message = "Failed to deserialize create response"
                    };
                }
                else
                {
                    _logger.LogWarning($"Failed to create menu item. Status: {response.StatusCode}");
                    return new ApiResponse<MenuItem>
                    {
                        Success = false,
                        Message = $"Failed to create menu item. Status: {response.StatusCode}",
                        Errors = new List<string> { content }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating menu item: {ex.Message}");
                return new ApiResponse<MenuItem>
                {
                    Success = false,
                    Message = "Error creating menu item",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<MenuItem>> UpdateMenuItemAsync(int id, CreateMenuItemDto menuItem)
        {
            try
            {
                _logger.LogInformation($"Updating menu item {id}: {System.Text.Json.JsonSerializer.Serialize(menuItem)}");
                
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsJsonAsync($"api/MenuItem/{id}", menuItem);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Update menu item response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<MenuItem>>();
                    return result ?? new ApiResponse<MenuItem>
                    {
                        Success = false,
                        Message = "Failed to deserialize update response"
                    };
                }
                else
                {
                    _logger.LogWarning($"Failed to update menu item. Status: {response.StatusCode}");
                    return new ApiResponse<MenuItem>
                    {
                        Success = false,
                        Message = $"Failed to update menu item. Status: {response.StatusCode}",
                        Errors = new List<string> { content }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating menu item: {ex.Message}");
                return new ApiResponse<MenuItem>
                {
                    Success = false,
                    Message = "Error updating menu item",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> DeleteMenuItemAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting menu item {id}");
                
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync($"api/MenuItem/{id}");
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Delete menu item response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                    return result ?? new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Failed to deserialize delete response"
                    };
                }
                else
                {
                    _logger.LogWarning($"Failed to delete menu item. Status: {response.StatusCode}");
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"Failed to delete menu item. Status: {response.StatusCode}",
                        Errors = new List<string> { content }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting menu item: {ex.Message}");
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting menu item",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
} 