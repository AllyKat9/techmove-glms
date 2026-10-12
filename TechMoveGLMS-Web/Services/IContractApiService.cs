using TechMoveGLMS_Web.Data.DTOs;
using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS_Web.Services
{
    public interface IContractApiService
    {
        Task<List<ContractResponseDto>> GetContractsAsync(ContractStatus? status = null, int? clientId = null);
        Task<ContractResponseDto> GetContractByIdAsync(int id);
        Task<Contract> CreateContractAsync(ContractCreateDto contract, IFormFile? signedAgreement = null);
        Task<bool> UpdateContractStatusAsync(int id, ContractStatus status);
        Task<FileDownloadResult?> DownloadAgreementAsync(int contractId);
        Task<bool> DeleteContractAsync(int id);
    }

    public class FileDownloadResult
    {
        public byte[] FileBytes { get; set; }
        public string OriginalFileName { get; set; }
        public string ContentType { get; set; }
    }
}
