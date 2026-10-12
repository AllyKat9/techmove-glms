using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS.API.DTOs
{
    // DTO for creating a new contract with file upload support
    public class ContractDtos
    {
        public int ClientId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; }
        public ServiceLevel ServiceLevel { get; set; }
        public IFormFile? SignedAgreement { get; set; }
    }

    // DTO for updating only the contract status
    public class ContractUpdateStatusDto
    {
        public ContractStatus Status { get; set; }
    }

    // DTO for returning contract data to API clients
    public class ContractResponseDto
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientRegion { get; set; }      
        public string ClientContactDetails { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ContractStatus Status { get; set; }
        public ServiceLevel ServiceLevel { get; set; }
        public string? SignedAgreementPath { get; set; }
        public string? OriginalFileName { get; set; }
        public bool IsActive { get; set; }
        public bool CanCreateServiceRequest { get; set; }
    }
}
