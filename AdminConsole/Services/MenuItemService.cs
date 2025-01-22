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

        public MenuItemService(
            HttpClient httpClient,
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
                return await _httpClient.GetFromJsonAsync<ApiResponse<List<MenuItem>>>("api/menuitem");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<MenuItem>>
                {
                    Success = false,
                    Message = $"Error retrieving menu items: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<MenuItem>> GetMenuItemAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<ApiResponse<MenuItem>>($"api/menuitem/{id}");
            }
            catch (Exception ex)
            {
                return new ApiResponse<MenuItem>
                {
                    Success = false,
                    Message = $"Error retrieving menu item: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<MenuItem>> CreateMenuItemAsync(CreateMenuItemDto model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/menuitem", model);
                return await response.Content.ReadFromJsonAsync<ApiResponse<MenuItem>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<MenuItem>
                {
                    Success = false,
                    Message = $"Error creating menu item: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<MenuItem>> UpdateMenuItemAsync(int id, CreateMenuItemDto model)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/menuitem/{id}", model);
                return await response.Content.ReadFromJsonAsync<ApiResponse<MenuItem>>();
            }
            catch (Exception ex)
            {
                return new ApiResponse<MenuItem>
                {
                    Success = false,
                    Message = $"Error updating menu item: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<bool>> DeleteMenuItemAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/menuitem/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Menu item deleted successfully"
                    };
                }
                else
                {
                    return new ApiResponse<bool>
                    {
                        Success = false,
                        Data = false,
                        Message = "Failed to delete menu item"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Data = false,
                    Message = $"Error deleting menu item: {ex.Message}"
                };
            }
        }
    }
} 