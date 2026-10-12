using Microsoft.EntityFrameworkCore;
using TechMoveGLMS_Web.Data;
using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS.API.Services
{
    public class ContractRepository : IContractRepository
    {
        private readonly ApplicationDbContext _context;

        public ContractRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Contract?> GetByIdAsync(int id)
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.ServiceRequests)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Contract>> GetActiveContractsAsync()
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.Status == ContractStatus.Active &&
                           c.StartDate <= DateTime.Now &&
                           c.EndDate >= DateTime.Now)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId)
        {
            return await _context.Contracts
                .Include(c => c.Client)
                .Where(c => c.ClientId == clientId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Contract>> GetByDateRangeAndStatusAsync(DateTime startDate, DateTime endDate, ContractStatus? status)
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            query = query.Where(c => c.StartDate >= startDate && c.EndDate <= endDate);

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Contract> AddAsync(Contract contract)
        {
            _context.Contracts.Add(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<Contract> UpdateAsync(Contract contract)
        {
            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();
            return contract;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var contract = await GetByIdAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public bool CanCreateServiceRequest(Contract contract)
        {
            return contract != null &&
                   contract.Status != ContractStatus.Expired &&
                   contract.Status != ContractStatus.OnHold;
        }
    }
}
