using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMoveGLMS_Web.Models
{
    public enum ContractStatus
    {
      // represents the contract's status 
        Draft,
        Active,
        Expired,
        OnHold
    }

    public enum ServiceLevel
    {
      // defines available service tiers for contracts
        Basic,
        Standard,
        Premium,
        Enterprise
    }

    public class Contract
    {
        [Key]
        public int Id { get; set; }

        // Contract entities
        [Required]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        [Required]
        public ServiceLevel ServiceLevel { get; set; }

        [StringLength(500)]
        public string? SignedAgreementPath { get; set; }

        public string? OriginalFileName { get; set; }


        // Navigation property -> lists service requests associated with the contract
        public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

        // Check if contract is currently active
        public bool IsActive()
        {
            return Status == ContractStatus.Active &&
                   DateTime.Now >= StartDate &&
                   DateTime.Now <= EndDate;
        }

        // Determines if the contract can get new service requests
        public bool CanCreateServiceRequest()
        {
            return Status != ContractStatus.Expired && Status != ContractStatus.OnHold;
        }
    }
}
