namespace TechMoveGLMS_Web.Services
{
    public interface IApiService
    {
        // This service interface is for making authenticated HTTP requests (GET, POST, PUT, PATCH, DELETE) to an API
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PostMultipartAsync<T>(string endpoint, MultipartFormDataContent content);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task<T> PatchAsync<T>(string endpoint, object data);
        Task<byte[]> GetBytesAsync(string endpoint);
        Task<bool> DeleteAsync(string endpoint);
        void SetAuthToken(string token);
    }
}
