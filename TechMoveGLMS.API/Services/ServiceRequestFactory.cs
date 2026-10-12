using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS.API.Services
{
    public class ServiceRequestFactory
    {
        private readonly IContractRepository _contractRepository;

        public ServiceRequestFactory(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        public async Task<(bool success, ServiceRequest? request, string errorMessage)> CreateRequestAsync(
            int contractId,
            string description,
            decimal costUSD,
            decimal exchangeRate)
        {
            var contract = await _contractRepository.GetByIdAsync(contractId);

            if (contract == null)
            {
                return (false, null, "Contract not found");
            }

            if (!_contractRepository.CanCreateServiceRequest(contract))
            {
                return (false, null, $"Cannot create service request. Contract status is {contract.Status}");
            }

            var serviceRequest = new ServiceRequest
            {
                ContractId = contractId,
                Description = description,
                CostUSD = costUSD,
                CostZAR = costUSD * exchangeRate,
                ExchangeRateUsed = exchangeRate,
                Status = RequestStatus.Pending,
                CreatedAt = DateTime.Now
            };

            return (true, serviceRequest, string.Empty);
        }
    }
}
