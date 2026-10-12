using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace TechMoveGLMS.Tests.Tests.Integration
{
    public class IntegrationTestBase : IClassFixture<WebApplicationFactory<TechMoveGLMS.API.Program>>
    {
        protected readonly HttpClient _client;
        protected readonly JsonSerializerOptions _jsonOptions;
        private string _authToken;

        public IntegrationTestBase(WebApplicationFactory<TechMoveGLMS.API.Program> factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // Helper method to get authentication token
        protected async Task<string> GetAuthTokenAsync()
        {
            if (!string.IsNullOrEmpty(_authToken))
                return _authToken;

            var username = Environment.GetEnvironmentVariable("TEST_ADMIN_USERNAME") ?? "admin";
            var password = Environment.GetEnvironmentVariable("TEST_ADMIN_PASSWORD") ?? "ChangeMeInTests";

            var loginData = new { username, password };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginData);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginTestResponse>();
                _authToken = result?.Token;
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authToken);
                return _authToken;
            }

            // If login fails, still try without auth for GET requests
            return null;
        }

        protected async Task<HttpResponseMessage> AuthorizedGetAsync(string endpoint)
        {
            try
            {
                await GetAuthTokenAsync();
            }
            catch { /* Continue without auth for GET */ }
            return await _client.GetAsync(endpoint);
        }

        protected async Task<HttpResponseMessage> AuthorizedPostAsync(string endpoint, object data)
        {
            await GetAuthTokenAsync();
            return await _client.PostAsJsonAsync(endpoint, data);
        }

        protected async Task<HttpResponseMessage> AuthorizedPatchAsync(string endpoint, object data)
        {
            await GetAuthTokenAsync();
            var content = JsonContent.Create(data);
            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
            {
                Content = content
            };
            return await _client.SendAsync(request);
        }

        protected async Task<HttpResponseMessage> AuthorizedDeleteAsync(string endpoint)
        {
            await GetAuthTokenAsync();
            return await _client.DeleteAsync(endpoint);
        }
    }

    public class LoginTestResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Username { get; set; }
    }
}
