namespace TechMoveGLMS_Web.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        string GetToken();
        Task<string> GetTokenAsync();
    }
}
