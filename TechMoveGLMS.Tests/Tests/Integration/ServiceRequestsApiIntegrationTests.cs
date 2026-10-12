using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace TechMoveGLMS.Tests.Tests.Integration
{
    public class ServiceRequestsApiIntegrationTests : IntegrationTestBase
    {
        public ServiceRequestsApiIntegrationTests(WebApplicationFactory<TechMoveGLMS.API.Program> factory)
            : base(factory) { }

        [Fact]
        public async Task GetServiceRequests_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await AuthorizedGetAsync("/api/ServiceRequests");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetExchangeRate_ReturnsValidRate()
        {
            // Act
            var response = await _client.GetAsync("/api/ServiceRequests/exchange-rate");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var rateData = await response.Content.ReadFromJsonAsync<ExchangeRateTestResponse>();
            Assert.NotNull(rateData);
            Assert.Equal("USD", rateData.FromCurrency);
            Assert.Equal("ZAR", rateData.ToCurrency);
            Assert.True(rateData.Rate > 0);
        }

        [Fact]
        public async Task CreateServiceRequest_WithValidContract_ReturnsSuccess()
        {
            // Arrange - First ensure we have an active contract
            var activeContractsResponse = await AuthorizedGetAsync("/api/contracts?status=1");
            var activeContracts = await activeContractsResponse.Content.ReadFromJsonAsync<List<ContractTestResponse>>();

            if (activeContracts == null || !activeContracts.Any())
            {
                // Create an active contract first
                var newContract = new
                {
                    clientId = 1,
                    startDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    endDate = DateTime.UtcNow.AddDays(335).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    status = 1,
                    serviceLevel = 2
                };
                var createContractResponse = await AuthorizedPostAsync("/api/contracts", newContract);
                activeContracts = new List<ContractTestResponse>
                {
                    await createContractResponse.Content.ReadFromJsonAsync<ContractTestResponse>()
                };
            }

            var contractId = activeContracts.First().Id;

            // Arrange - Service request data
            var newRequest = new
            {
                contractId = contractId,
                description = "Integration test service request",
                costUSD = 500.00m
            };

            // Act
            var createResponse = await AuthorizedPostAsync("/api/ServiceRequests", newRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var createdRequest = await createResponse.Content.ReadFromJsonAsync<ServiceRequestTestResponse>();
            Assert.NotNull(createdRequest);
            Assert.Equal(contractId, createdRequest.ContractId);
            Assert.Equal("Integration test service request", createdRequest.Description);
            Assert.Equal(500.00m, createdRequest.CostUSD);

            // Verify ZAR conversion
            Assert.True(createdRequest.CostZAR > 0);
            Assert.Equal(RequestStatusTest.Pending, createdRequest.Status);
        }

        [Fact]
        public async Task CreateServiceRequest_WithExpiredContract_ReturnsBadRequest()
        {
            // Try to create service request with an expired contract (id=2 from seed data)
            var newRequest = new
            {
                contractId = 2, // Expired contract from seed data
                description = "This should fail",
                costUSD = 100.00m
            };

            // Act
            var createResponse = await AuthorizedPostAsync("/api/ServiceRequests", newRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, createResponse.StatusCode);

            var errorResponse = await createResponse.Content.ReadFromJsonAsync<ErrorTestResponse>();
            Assert.NotNull(errorResponse);
            Assert.Contains("Cannot create service request", errorResponse.message);
        }
    }

    public class ExchangeRateTestResponse
    {
        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Rate { get; set; }
    }

    public class ServiceRequestTestResponse
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string ContractServiceLevel { get; set; }
        public string ClientName { get; set; }
        public string Description { get; set; }
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public RequestStatusTest Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal ExchangeRateUsed { get; set; }
    }

    public enum RequestStatusTest
    {
        Pending,
        Approved,
        InProgress,
        Completed,
        Cancelled
    }

    public class ErrorTestResponse
    {
        public string message { get; set; }
    }
}
