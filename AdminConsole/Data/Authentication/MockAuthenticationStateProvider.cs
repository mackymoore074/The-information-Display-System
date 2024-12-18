using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;

namespace AdminConsole.Data.Authentication
{
    public class MockAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private AuthenticationState _anonymous => new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        public MockAuthenticationStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var localStorageToken = await _localStorage.GetItemAsync<string>("authToken");
            
            if (string.IsNullOrEmpty(localStorageToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var loginData = await _localStorage.GetItemAsync<LoginData>("loginData");
            if (loginData == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, loginData.Email),
                new Claim(ClaimTypes.Role, loginData.Role),
                new Claim("AdminId", loginData.AdminId.ToString())
            };

            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task MarkUserAsAuthenticated(LoginData loginData)
        {
            var userData = new UserData
            {
                AdminId = loginData.AdminId,
                Email = loginData.Email,
                FirstName = loginData.FirstName,
                LastName = loginData.LastName,
                Role = loginData.Role
            };

            await _localStorage.SetItemAsync("authToken", loginData.Token);
            await _localStorage.SetItemAsync("user", userData);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task MarkUserAsLoggedOut()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("user");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    public class UserData
    {
        public int AdminId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class LoginData
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int AdminId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
