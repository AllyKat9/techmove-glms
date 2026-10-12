using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace TechMoveGLMS.Tests.Tests.Integration
{
    public class ContractsApiIntegrationTests : IntegrationTestBase
    {
        public ContractsApiIntegrationTests(WebApplicationFactory<TechMoveGLMS.API.Program> factory)
            : base(factory) { }

        [Fact]
        public async Task GetContracts_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await AuthorizedGetAsync("/api/contracts");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetContracts_ReturnsJsonArray()
        {
            // Act
            var response = await AuthorizedGetAsync("/api/contracts");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.StartsWith("[", content.Trim());
            Assert.EndsWith("]", content.Trim());
        }

        [Fact]
        public async Task GetContractById_WithValidId_ReturnsContract()
        {
            // Act
            var response = await AuthorizedGetAsync("/api/contracts/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var contract = await response.Content.ReadFromJsonAsync<ContractTestResponse>();
            Assert.NotNull(contract);
            Assert.Equal(1, contract.Id);
        }

        [Fact]
        public async Task GetContractById_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await AuthorizedGetAsync("/api/contracts/9999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateContract_ThenRead_VerifiesDataIntegrity()
        {
            // Arrange - Use valid future dates
            var startDate = DateTime.UtcNow.AddDays(30);
            var endDate = DateTime.UtcNow.AddDays(395);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("1"), "ClientId");
            formData.Add(new StringContent(startDate.ToString("yyyy-MM-ddTHH:mm:ssZ")), "StartDate");
            formData.Add(new StringContent(endDate.ToString("yyyy-MM-ddTHH:mm:ssZ")), "EndDate");
            formData.Add(new StringContent("1"), "Status"); // Active
            formData.Add(new StringContent("2"), "ServiceLevel"); // Premium

            // Act - Create
            await GetAuthTokenAsync();
            var createResponse = await _client.PostAsync("/api/contracts", formData);

            // Assert - Create successful
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var createdContract = await createResponse.Content.ReadFromJsonAsync<ContractTestResponse>();
            Assert.NotNull(createdContract);
            Assert.True(createdContract.Id > 0);

            // Act - Read the created contract
            var getResponse = await AuthorizedGetAsync($"/api/contracts/{createdContract.Id}");

            // Assert - Read successful and data matches
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var retrievedContract = await getResponse.Content.ReadFromJsonAsync<ContractTestResponse>();
            Assert.NotNull(retrievedContract);
            Assert.Equal(createdContract.Id, retrievedContract.Id);
            Assert.Equal(createdContract.ClientId, retrievedContract.ClientId);
        }

        [Fact]
        public async Task UpdateContractStatus_WithValidData_ReturnsSuccess()
        {
            // Arrange - First create a draft contract with valid dates
            var startDate = DateTime.UtcNow.AddDays(30);
            var endDate = DateTime.UtcNow.AddDays(395);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("1"), "ClientId");
            formData.Add(new StringContent(startDate.ToString("yyyy-MM-ddTHH:mm:ssZ")), "StartDate");
            formData.Add(new StringContent(endDate.ToString("yyyy-MM-ddTHH:mm:ssZ")), "EndDate");
            formData.Add(new StringContent("0"), "Status"); // Draft
            formData.Add(new StringContent("1"), "ServiceLevel"); // Standard

            await GetAuthTokenAsync();
            var createResponse = await _client.PostAsync("/api/contracts", formData);

            // Skip test if creation failed
            if (createResponse.StatusCode != HttpStatusCode.Created)
            {
                var error = await createResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Failed to create test contract: {error}");
                return;
            }

            var createdContract = await createResponse.Content.ReadFromJsonAsync<ContractTestResponse>();
            Assert.NotNull(createdContract);

            // Act - Update status to Active
            var updateData = new { status = 1 }; // Active
            var patchResponse = await AuthorizedPatchAsync($"/api/contracts/{createdContract.Id}/status", updateData);

            // Assert
            Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteContract_WithValidId_ReturnsNoContent()
        {
            // Arrange - First create a contract to delete with valid dates
            var startDate = DateTime.UtcNow.AddDays(30);
            var endDate = DateTime.UtcNow.AddDays(395);

            var formData = new MultipartFormDataContent();
            formData.Add(new StringContent("1"), "ClientId");
            formData.Add(new StringContent(startDate.ToString("yyyy-MM-ddTHH:mm:ssZ")), "StartDate");
            formData.Add(new StringContent(endDate.ToString("yyyy-MM-ddTHH:mm:ssZ")), "EndDate");
            formData.Add(new StringContent("0"), "Status");
            formData.Add(new StringContent("1"), "ServiceLevel");

            await GetAuthTokenAsync();
            var createResponse = await _client.PostAsync("/api/contracts", formData);

            if (createResponse.StatusCode != HttpStatusCode.Created)
            {
                var error = await createResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Failed to create test contract: {error}");
                return;
            }

            var createdContract = await createResponse.Content.ReadFromJsonAsync<ContractTestResponse>();
            Assert.NotNull(createdContract);

            // Act - Delete
            var deleteResponse = await AuthorizedDeleteAsync($"/api/contracts/{createdContract.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task GetContracts_WithStatusFilter_ReturnsFilteredResults()
        {
            // Act
            var response = await AuthorizedGetAsync("/api/contracts?status=1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var contracts = await response.Content.ReadFromJsonAsync<List<ContractTestResponse>>();
            Assert.NotNull(contracts);

            // All returned contracts should be Active (status = 1)
            foreach (var contract in contracts)
            {
                Assert.Equal(1, (int)contract.Status);
            }
        }

        [Fact]
        public async Task GetContracts_WithClientFilter_ReturnsFilteredResults()
        {
            // Act
            var response = await AuthorizedGetAsync("/api/contracts?clientId=1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var contracts = await response.Content.ReadFromJsonAsync<List<ContractTestResponse>>();
            Assert.NotNull(contracts);

            // All returned contracts should have clientId = 1
            foreach (var contract in contracts)
            {
                Assert.Equal(1, contract.ClientId);
            }
        }
    }

    // DTO for test deserialization
    public class ContractTestResponse
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
        public int ServiceLevel { get; set; }
        public string SignedAgreementPath { get; set; }
        public string OriginalFileName { get; set; }
        public bool IsActive { get; set; }
        public bool CanCreateServiceRequest { get; set; }
    }
}
