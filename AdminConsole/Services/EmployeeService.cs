using AdminConsole.IService;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Employee;
using Blazored.LocalStorage;
using System.Net.Http.Json;
using System.Text.Json;

namespace AdminConsole.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EmployeeService> _logger;
        private readonly ILocalStorageService _localStorage;

        public EmployeeService(HttpClient httpClient, 
            ILogger<EmployeeService> logger,
            ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _logger = logger;
            _localStorage = localStorage;
        }

        public async Task<ApiResponse<IEnumerable<EmployeeDto>>> GetEmployeesAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<IEnumerable<EmployeeDto>>>("api/Employee");
                return response ?? new ApiResponse<IEnumerable<EmployeeDto>> 
                { 
                    Success = false, 
                    Message = "Failed to retrieve employees" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetEmployeesAsync: {ex.Message}");
                return new ApiResponse<IEnumerable<EmployeeDto>> 
                { 
                    Success = false, 
                    Message = "Error retrieving employees",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<EmployeeDto>> GetEmployeeAsync(int id)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetFromJsonAsync<ApiResponse<EmployeeDto>>($"api/Employee/{id}");
                return response ?? new ApiResponse<EmployeeDto> 
                { 
                    Success = false, 
                    Message = "Failed to retrieve employee" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetEmployeeAsync: {ex.Message}");
                return new ApiResponse<EmployeeDto> 
                { 
                    Success = false, 
                    Message = "Error retrieving employee",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<EmployeeDto>> CreateEmployeeAsync(CreateEmployeeDto employee)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                _logger.LogInformation($"Sending POST request to api/Employee with data: {JsonSerializer.Serialize(employee)}");

                var response = await _httpClient.PostAsJsonAsync("api/Employee", employee);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Received response: {responseContent}");

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
                    return result ?? new ApiResponse<EmployeeDto> 
                    { 
                        Success = false, 
                        Message = "Failed to deserialize response" 
                    };
                }
                else
                {
                    return new ApiResponse<EmployeeDto> 
                    { 
                        Success = false, 
                        Message = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}",
                        Errors = new List<string> { responseContent }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateEmployeeAsync: {ex.Message}");
                return new ApiResponse<EmployeeDto> 
                { 
                    Success = false, 
                    Message = "Error creating employee",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<EmployeeDto>> UpdateEmployeeAsync(int id, UpdateEmployeeDto employee)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsJsonAsync($"api/Employee/{id}", employee);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<EmployeeDto>>();
                return result ?? new ApiResponse<EmployeeDto> 
                { 
                    Success = false, 
                    Message = "Failed to update employee" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateEmployeeAsync: {ex.Message}");
                return new ApiResponse<EmployeeDto> 
                { 
                    Success = false, 
                    Message = "Error updating employee",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<object>> DeleteEmployeeAsync(int id)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync($"api/Employee/{id}");
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                return result ?? new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "Failed to delete employee" 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteEmployeeAsync: {ex.Message}");
                return new ApiResponse<object> 
                { 
                    Success = false, 
                    Message = "Error deleting employee",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
} 