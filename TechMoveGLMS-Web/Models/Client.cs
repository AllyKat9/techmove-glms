using System.ComponentModel.DataAnnotations;

namespace TechMoveGLMS_Web.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        // Client Entinties
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        public string ContactDetails { get; set; }

        [Required]
        [StringLength(50)]
        public string Region { get; set; }

        // Navigation property -> Lists all contracts by a specific client
        public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
