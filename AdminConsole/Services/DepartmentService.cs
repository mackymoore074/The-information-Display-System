using System.Net.Http;
using System.Text.Json;
using ClassLibrary.DtoModels.Department;
using ClassLibrary.DtoModels.Common;
using Microsoft.Extensions.Logging;
using AdminConsole.IService;
using Blazored.LocalStorage;

namespace AdminConsole.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<DepartmentService> _logger;
        private readonly ILocalStorageService _localStorage;
        private readonly JsonSerializerOptions _jsonOptions;

        public DepartmentService(HttpClient httpClient, ILogger<DepartmentService> logger, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _logger = logger;
            _localStorage = localStorage;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponse<IEnumerable<DepartmentDto>>> GetDepartmentsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all departments");
                
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/Department");
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Department response content: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<DepartmentDto>>>();
                    return result ?? new ApiResponse<IEnumerable<DepartmentDto>>
                    {
                        Success = false,
                        Message = "Failed to deserialize departments response"
                    };
                }
                else
                {
                    _logger.LogWarning($"Failed to get departments. Status: {response.StatusCode}");
                    return new ApiResponse<IEnumerable<DepartmentDto>>
                    {
                        Success = false,
                        Message = $"Failed to get departments. Status: {response.StatusCode}",
                        Errors = new List<string> { content }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting departments: {ex.Message}");
                return new ApiResponse<IEnumerable<DepartmentDto>>
                {
                    Success = false,
                    Message = "Error getting departments",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"/api/department/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<DepartmentDto>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}"
                    };
                }

                return JsonSerializer.Deserialize<ApiResponse<DepartmentDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDepartmentByIdAsync: {ex.Message}", ex);
                return new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Error retrieving department"
                };
            }
        }

        public async Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto createDepartmentDto)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("/api/department", createDepartmentDto);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<DepartmentDto>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}"
                    };
                }

                return JsonSerializer.Deserialize<ApiResponse<DepartmentDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateDepartmentAsync: {ex.Message}", ex);
                return new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Error creating department"
                };
            }
        }

        public async Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDepartmentDto)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsJsonAsync($"/api/department/{id}", updateDepartmentDto);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<DepartmentDto>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}"
                    };
                }

                return JsonSerializer.Deserialize<ApiResponse<DepartmentDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateDepartmentAsync: {ex.Message}", ex);
                return new ApiResponse<DepartmentDto>
                {
                    Success = false,
                    Message = "Error updating department"
                };
            }
        }

        public async Task<ApiResponse<object>> DeleteDepartmentAsync(int id)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync($"/api/department/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}"
                    };
                }

                return JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteDepartmentAsync: {ex.Message}", ex);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting department"
                };
            }
        }

        // Implement other interface methods similarly...
    }
} 