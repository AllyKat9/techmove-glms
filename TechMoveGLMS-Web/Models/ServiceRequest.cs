using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMoveGLMS_Web.Models
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        InProgress,
        Completed,
        Cancelled
    }

    public class ServiceRequest
    {
        [Key]
        public int Id { get; set; }

        // Service request entities
        [Required]
        public int ContractId { get; set; }

        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostUSD { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal CostZAR { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,4)")]
        public decimal ExchangeRateUsed { get; set; }
    }
}
