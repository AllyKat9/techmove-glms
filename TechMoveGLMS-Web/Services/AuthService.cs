using System.Text.Json;

namespace TechMoveGLMS_Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IApiService apiService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthService> logger)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var loginData = new { username, password };
                var response = await _apiService.PostAsync<LoginResponse>("/api/auth/login", loginData);

                if (response?.Token != null)
                {
                    // Store token in session
                    _httpContextAccessor.HttpContext.Session.SetString("AuthToken", response.Token);
                    _apiService.SetAuthToken(response.Token);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            _httpContextAccessor.HttpContext.Session.Remove("AuthToken");
            _apiService.SetAuthToken(null);
            await Task.CompletedTask;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await GetTokenAsync();
            return !string.IsNullOrEmpty(token);
        }

        public string GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session?.GetString("AuthToken");
        }

        public async Task<string> GetTokenAsync()
        {
            var token = GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                _apiService.SetAuthToken(token);
            }
            return token;
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Username { get; set; }
    }
}

