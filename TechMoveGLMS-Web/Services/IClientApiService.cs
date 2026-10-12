using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS_Web.Services
{
    public interface IClientApiService
    {
        Task<List<Client>> GetClientsAsync();
        Task<Client> GetClientByIdAsync(int id);
        Task<Client> CreateClientAsync(Client client);
        Task<bool> UpdateClientAsync(Client client);
        Task<bool> DeleteClientAsync(int id);

    }
}
