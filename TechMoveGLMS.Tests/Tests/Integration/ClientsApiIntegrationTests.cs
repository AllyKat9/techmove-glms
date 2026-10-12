using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace TechMoveGLMS.Tests.Tests.Integration
{
    public class ClientsApiIntegrationTests : IntegrationTestBase
    {
        public ClientsApiIntegrationTests(WebApplicationFactory<TechMoveGLMS.API.Program> factory)
            : base(factory) { }

        [Fact]
        public async Task GetClients_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await AuthorizedGetAsync("/api/clients");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetClients_ReturnsJsonArray()
        {
            // Act
            var response = await AuthorizedGetAsync("/api/clients");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.StartsWith("[", content.Trim());
            Assert.EndsWith("]", content.Trim());
        }

        [Fact]
        public async Task CreateClient_ThenRead_VerifiesDataIntegrity()
        {
            // Arrange
            var newClient = new
            {
                name = "Integration Test Client",
                contactDetails = "test@integration.com",
                region = "Test Region"
            };

            // Act - Create
            await GetAuthTokenAsync();
            var createResponse = await AuthorizedPostAsync("/api/clients", newClient);

            // Assert - Create successful
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var createdClient = await createResponse.Content.ReadFromJsonAsync<ClientTestResponse>();

            Assert.NotNull(createdClient);
            Assert.True(createdClient.Id > 0);

            // Act - Read the created client
            var getResponse = await AuthorizedGetAsync($"/api/clients/{createdClient.Id}");

            // Assert - Read successful and data matches
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var retrievedClient = await getResponse.Content.ReadFromJsonAsync<ClientTestResponse>();

            Assert.NotNull(retrievedClient);
            Assert.Equal(createdClient.Id, retrievedClient.Id);
            Assert.Equal(createdClient.Name, retrievedClient.Name);
        }

        [Fact]
        public async Task GetClientById_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await AuthorizedGetAsync("/api/clients/9999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateClient_WithValidData_ReturnsSuccess()
        {
            // Arrange - First create a client
            var newClient = new
            {
                name = "Original Name",
                contactDetails = "original@email.com",
                region = "Original Region"
            };

            await GetAuthTokenAsync();
            var createResponse = await _client.PostAsJsonAsync("/api/clients", newClient);

            if (createResponse.StatusCode != HttpStatusCode.Created)
            {
                var error = await createResponse.Content.ReadAsStringAsync();
                Assert.Fail($"Failed to create test client: {error}");
                return;
            }

            var createdClient = await createResponse.Content.ReadFromJsonAsync<ClientTestResponse>();
            Assert.NotNull(createdClient);

            // Act - Update client using PUT
            var updateData = new
            {
                name = "Updated Name",
                contactDetails = "updated@email.com",
                region = "Updated Region"
            };
            var putResponse = await _client.PutAsJsonAsync($"/api/clients/{createdClient.Id}", updateData);

            // Assert
            Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);
        }

    }

    public class ClientTestResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactDetails { get; set; }
        public string Region { get; set; }
        public int ContractCount { get; set; }
    }
}
