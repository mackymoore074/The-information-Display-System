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
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                if (string.IsNullOrEmpty(token))
                {
                    return _anonymous;
                }

                var user = await _localStorage.GetItemAsync<UserData>("user");
                if (user == null)
                {
                    return _anonymous;
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("AdminId", user.AdminId.ToString())
                };

                var identity = new ClaimsIdentity(claims, "Bearer");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
            catch (InvalidOperationException)
            {
                return _anonymous;
            }
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
