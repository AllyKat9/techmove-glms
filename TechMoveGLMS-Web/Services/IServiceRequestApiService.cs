using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS_Web.Services
{
    public interface IServiceRequestApiService
    {
        Task<List<ServiceRequest>> GetServiceRequestsAsync();
        Task<ServiceRequest> GetServiceRequestByIdAsync(int id);
        Task<ServiceRequest> CreateServiceRequestAsync(int contractId, string description, decimal costUSD);
        Task<decimal> GetExchangeRateAsync();
    }
}
