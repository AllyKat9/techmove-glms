using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS.API.DTOs
{
    // DTO for creating a new service request [input model]
    public class ServiceRequestCreateDto
    {
        public int ContractId { get; set; }
        public string Description { get; set; }
        public decimal CostUSD { get; set; }
    }

    // DTO for returning service request data [output model]
    public class ServiceRequestResponseDto
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public string ContractServiceLevel { get; set; }
        public string ClientName { get; set; }
        public string Description { get; set; }
        public decimal CostUSD { get; set; }
        public decimal CostZAR { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal ExchangeRateUsed { get; set; }
    }
}
