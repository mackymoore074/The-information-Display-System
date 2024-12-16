using System.Net.Http;
using System.Threading.Tasks;

public class ApiService
{
    private readonly HttpClient _httpClient;

    // No need to inject ILocalStorageService anymore, since the handler handles the token
    public ApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetDataAsync()
    {
        try
        {
            // Make the HTTP GET request to the API
            var response = await _httpClient.GetAsync("https://localhost:7187");

            // Check if the request was successful
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new HttpRequestException($"Failed to fetch data: {response.ReasonPhrase}");
            }
        }
        catch (Exception ex)
        {
            // Log the error or handle it as needed
            throw new HttpRequestException($"An error occurred: {ex.Message}");
        }
    }
}
