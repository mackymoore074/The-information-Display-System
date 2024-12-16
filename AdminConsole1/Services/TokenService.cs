namespace AdminConsole.Services
{
    public interface ITokenService
    {
        Task<string> GetTokenAsync();
    }

    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetTokenAsync()
        {
            // Fetch the token from secure storage or a backend service
            var token = _httpContextAccessor.HttpContext.Session.GetString("AuthToken");
            if (string.IsNullOrEmpty(token))
            {
                // Logic to refresh token or throw an error
            }
            return token;
        }
    }
}
