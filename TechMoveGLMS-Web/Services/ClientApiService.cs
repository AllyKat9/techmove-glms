using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS_Web.Services
{
    public class ClientApiService : IClientApiService
    {
        private readonly IApiService _apiService;

        public ClientApiService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<Client>> GetClientsAsync()
        {
            return await _apiService.GetAsync<List<Client>>("/api/clients");
        }

        public async Task<Client> GetClientByIdAsync(int id)
        {
            return await _apiService.GetAsync<Client>($"/api/clients/{id}");
        }

        public async Task<Client> CreateClientAsync(Client client)
        {
            return await _apiService.PostAsync<Client>("/api/clients", client);
        }

        public async Task<bool> UpdateClientAsync(Client client)
        {
            await _apiService.PutAsync<object>($"/api/clients/{client.Id}", client);
            return true;
        }

        public async Task<bool> DeleteClientAsync(int id)
        {
            return await _apiService.DeleteAsync($"/api/clients/{id}");
        }
    }
}
