using System.Text;
using System.Text.Json;

namespace TechMoveGLMS_Web.Services
{
    // Service for making HTTP API calls to the backend with authentication support
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiService> _logger;
        private string _authToken;

        public ApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // Sets the authentication token and adds it to HTTP request headers as Bearer token
        public void SetAuthToken(string token)
        {
            _authToken = token;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        // Ensures a valid auth token is set by retrieving from session if available
        private async Task EnsureTokenAsync()
        {
            // If no token is set, try to get it from session
            if (string.IsNullOrEmpty(_authToken))
            {
                var token = _httpContextAccessor.HttpContext?.Session?.GetString("AuthToken");
                if (!string.IsNullOrEmpty(token))
                {
                    SetAuthToken(token);
                }
            }
            await Task.CompletedTask;
        }


        // Performs a GET request to the specified endpoint and deserializes the response to type T
        public async Task<T> GetAsync<T>(string endpoint)
        {
            await EnsureTokenAsync();
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogError($"GET {endpoint} failed: {response.StatusCode}");
                throw new HttpRequestException($"API returned {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling GET {endpoint}");
                throw;
            }
        }

        // Performs a POST request with JSON data to the specified endpoint and returns deserialized response
        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            await EnsureTokenAsync();
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"POST {endpoint} failed: {response.StatusCode} - {error}");
                throw new HttpRequestException($"API returned {response.StatusCode}: {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling POST {endpoint}");
                throw;
            }
        }

        // Performs a POST with multipart/form-data (for file uploads) and deserializes the response
        public async Task<T> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            await EnsureTokenAsync();
            try
            {
                var response = await _httpClient.PostAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError($"POST(multipart) {endpoint} failed: {response.StatusCode} - {error}");
                throw new HttpRequestException($"API returned {response.StatusCode}: {error}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling POST(multipart) {endpoint}");
                throw;
            }
        }

        // Performs a PUT request with JSON data to update a resource at the specified endpoint
        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            await EnsureTokenAsync();
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogError($"PUT {endpoint} failed: {response.StatusCode}");
                throw new HttpRequestException($"API returned {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling PUT {endpoint}");
                throw;
            }
        }

        // Performs a PATCH request with JSON data for partial updates at the specified endpoint
        public async Task<T> PatchAsync<T>(string endpoint, object data)
        {
            await EnsureTokenAsync();
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
                {
                    Content = content
                };

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<T>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }

                _logger.LogError($"PATCH {endpoint} failed: {response.StatusCode}");
                throw new HttpRequestException($"API returned {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling PATCH {endpoint}");
                throw;
            }
        }

        // Downloads raw bytes (for files, images, PDFs) with authentication
        public async Task<byte[]> GetBytesAsync(string endpoint)
        {
            await EnsureTokenAsync();
            try
            {
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }

                _logger.LogError($"GET(bytes) {endpoint} failed: {response.StatusCode}");
                throw new HttpRequestException($"API returned {response.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling GET(bytes) {endpoint}");
                throw;
            }
        }

        // Performs a DELETE request to remove a resource at the specified endpoint, returns success status
        public async Task<bool> DeleteAsync(string endpoint)
        {
            await EnsureTokenAsync();
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling DELETE {endpoint}");
                return false;
            }
        }
    }
}
