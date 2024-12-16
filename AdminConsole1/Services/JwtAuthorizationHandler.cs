using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;

public class JwtAuthorizationHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public JwtAuthorizationHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Retrieve the JWT token from local storage
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrEmpty(token))
        {
            // Add the token to the Authorization header of the request
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Continue with the HTTP request
        return await base.SendAsync(request, cancellationToken);
    }
}

