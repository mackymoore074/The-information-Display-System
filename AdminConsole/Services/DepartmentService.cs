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

        public async Task<ApiResponse<List<DepartmentDto>>> GetDepartmentsAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("/api/department");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<List<DepartmentDto>>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}"
                    };
                }

                return JsonSerializer.Deserialize<ApiResponse<List<DepartmentDto>>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetDepartmentsAsync: {ex.Message}", ex);
                return new ApiResponse<List<DepartmentDto>>
                {
                    Success = false,
                    Message = "Error retrieving departments"
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