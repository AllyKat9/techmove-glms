using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS.API.Services
{
    public interface IContractRepository
    {
        Task<Contract?> GetByIdAsync(int id);
        Task<IEnumerable<Contract>> GetActiveContractsAsync();
        Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId);
        Task<IEnumerable<Contract>> GetByDateRangeAndStatusAsync(DateTime startDate, DateTime endDate, ContractStatus? status);
        Task<Contract> AddAsync(Contract contract);
        Task<Contract> UpdateAsync(Contract contract);
        Task<bool> DeleteAsync(int id);
        bool CanCreateServiceRequest(Contract contract);
    }
}
