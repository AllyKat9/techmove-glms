using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS_Web.Services
{
    public class ServiceRequestApiService : IServiceRequestApiService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<ServiceRequestApiService> _logger;

        public ServiceRequestApiService(IApiService apiService, ILogger<ServiceRequestApiService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<List<ServiceRequest>> GetServiceRequestsAsync()
        {
            return await _apiService.GetAsync<List<ServiceRequest>>("/api/ServiceRequests");
        }

        public async Task<ServiceRequest> GetServiceRequestByIdAsync(int id)
        {
            return await _apiService.GetAsync<ServiceRequest>($"/api/ServiceRequests/{id}");
        }

        public async Task<ServiceRequest> CreateServiceRequestAsync(int contractId, string description, decimal costUSD)
        {
            var data = new
            {
                contractId,
                description,
                costUSD
            };
            return await _apiService.PostAsync<ServiceRequest>("/api/ServiceRequests", data);
        }

        public async Task<decimal> GetExchangeRateAsync()
        {
            try
            {
                // Create a simple DTO for the API response
                var result = await _apiService.GetAsync<ExchangeRateApiResponse>("/api/ServiceRequests/exchange-rate");

                if (result != null && result.Rate > 0)
                {
                    return result.Rate;
                }

                return 19.50m;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting exchange rate");
                return 19.50m;
            }
        }
    }

    public class ExchangeRateApiResponse
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Rate { get; set; }
    }
}

