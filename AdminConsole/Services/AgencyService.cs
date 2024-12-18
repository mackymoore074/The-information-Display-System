using System.Net.Http;
using System.Text.Json;
using ClassLibrary.DtoModels.Agency;
using ClassLibrary.DtoModels.Common;
using ClassLibrary.DtoModels.Location;
using Microsoft.Extensions.Logging;
using AdminConsole.IService;
using Blazored.LocalStorage;

namespace AdminConsole.Services
{
    public class AgencyService : IAgencyService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AgencyService> _logger;
        private readonly ILocalStorageService _localStorage;
        private readonly JsonSerializerOptions _jsonOptions;

        public AgencyService(HttpClient httpClient, ILogger<AgencyService> logger, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _logger = logger;
            _localStorage = localStorage;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ApiResponse<List<AgencyDto>>> GetAgenciesAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                
                var response = await _httpClient.GetAsync("api/agency");
                var content = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<List<AgencyDto>>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}",
                        Data = new List<AgencyDto>()
                    };
                }

                return JsonSerializer.Deserialize<ApiResponse<List<AgencyDto>>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAgenciesAsync: {ex.Message}", ex);
                return new ApiResponse<List<AgencyDto>>
                {
                    Success = false,
                    Message = "Error retrieving agencies",
                    Data = new List<AgencyDto>()
                };
            }
        }

        public async Task<ApiResponse<AgencyDto>> GetAgencyByIdAsync(int id)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"api/agency/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<AgencyDto>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}"
                    };
                }

                return JsonSerializer.Deserialize<ApiResponse<AgencyDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAgencyByIdAsync: {ex.Message}", ex);
                return new ApiResponse<AgencyDto>
                {
                    Success = false,
                    Message = "Error retrieving agency"
                };
            }
        }

        public async Task<ApiResponse<AgencyDto>> CreateAgencyAsync(CreateAgencyDto createAgencyDto)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("api/agency", createAgencyDto);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Create response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<AgencyDto>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}",
                        Data = null
                    };
                }

                return JsonSerializer.Deserialize<ApiResponse<AgencyDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CreateAgencyAsync: {ex.Message}", ex);
                return new ApiResponse<AgencyDto>
                {
                    Success = false,
                    Message = "Error creating agency",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<AgencyDto>> UpdateAgencyAsync(int id, UpdateAgencyDto updateAgencyDto)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PutAsJsonAsync($"api/agency/{id}", updateAgencyDto);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Update response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<AgencyDto>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}",
                        Data = null
                    };
                }

                return JsonSerializer.Deserialize<ApiResponse<AgencyDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UpdateAgencyAsync: {ex.Message}", ex);
                return new ApiResponse<AgencyDto>
                {
                    Success = false,
                    Message = "Error updating agency",
                    Data = null
                };
            }
        }

        public async Task<ApiResponse<object>> DeleteAgencyAsync(int id)
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync($"api/agency/{id}");
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Delete response content: {content}");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"API returned {response.StatusCode}: {content}");
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = $"API error: {response.StatusCode}",
                        Data = null
                    };
                }

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "Agency deleted successfully",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DeleteAgencyAsync: {ex.Message}", ex);
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = "Error deleting agency",
                    Data = null
                };
            }
        }
    }
} 