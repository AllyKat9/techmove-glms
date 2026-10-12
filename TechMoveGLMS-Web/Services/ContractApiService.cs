using System.Configuration;
using System.Text.Json;
using TechMoveGLMS_Web.Data.DTOs;
using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS_Web.Services
{
    public class ContractApiService : IContractApiService
    {
        private readonly IApiService _apiService;
        private readonly ILogger<ContractApiService> _logger;

        public ContractApiService(IApiService apiService, ILogger<ContractApiService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<List<ContractResponseDto>> GetContractsAsync(ContractStatus? status = null, int? clientId = null)
        {
            var query = new List<string>();
            if (status.HasValue) query.Add($"status={(int)status.Value}");
            if (clientId.HasValue) query.Add($"clientId={clientId.Value}");

            var endpoint = "/api/contracts";
            if (query.Any()) endpoint += "?" + string.Join("&", query);

            return await _apiService.GetAsync<List<ContractResponseDto>>(endpoint);
        }

        public async Task<ContractResponseDto> GetContractByIdAsync(int id)
        {
            return await _apiService.GetAsync<ContractResponseDto>($"/api/contracts/{id}");
        }

        public async Task<Contract> CreateContractAsync(ContractCreateDto contract, IFormFile? signedAgreement = null)
        {
            using var formData = new MultipartFormDataContent();

            formData.Add(new StringContent(contract.ClientId.ToString()), "ClientId");
            formData.Add(new StringContent(contract.StartDate.ToString("yyyy-MM-ddTHH:mm:ssZ")), "StartDate");
            formData.Add(new StringContent(contract.EndDate.ToString("yyyy-MM-ddTHH:mm:ssZ")), "EndDate");
            formData.Add(new StringContent(((int)contract.Status).ToString()), "Status");
            formData.Add(new StringContent(((int)contract.ServiceLevel).ToString()), "ServiceLevel");

            if (signedAgreement != null)
            {
                var stream = signedAgreement.OpenReadStream();
                var fileContent = new StreamContent(stream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(signedAgreement.ContentType);
                formData.Add(fileContent, "SignedAgreement", signedAgreement.FileName);
            }

            // Route through IApiService so the JWT authentication header is attached
            return await _apiService.PostMultipartAsync<Contract>("/api/contracts", formData);
        }

        public async Task<bool> UpdateContractStatusAsync(int id, ContractStatus status)
        {
            var data = new { status = (int)status };
            await _apiService.PatchAsync<object>($"/api/contracts/{id}/status", data);
            return true;
        }

        public async Task<FileDownloadResult?> DownloadAgreementAsync(int contractId)
        {
            try
            {
                // First fetch contract metadata (for the original filename)
                var contract = await GetContractByIdAsync(contractId);
                if (contract == null || string.IsNullOrEmpty(contract.SignedAgreementPath))
                    return null;

                var bytes = await _apiService.GetBytesAsync($"/api/contracts/{contractId}/agreement");

                return new FileDownloadResult
                {
                    FileBytes = bytes,
                    OriginalFileName = contract.OriginalFileName ?? $"Contract_{contractId}_Agreement.pdf",
                    ContentType = "application/pdf"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading agreement for contract {Id}", contractId);
                return null;
            }
        }

        public async Task<bool> DeleteContractAsync(int id)
        {
            return await _apiService.DeleteAsync($"/api/contracts/{id}");
        }
    }
}
