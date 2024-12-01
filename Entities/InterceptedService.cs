using ShareProject.ApiPhantom.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShareProject.Entities
{
    public class InterceptedService
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("ServiceCatalog")]
        public Guid ServiceCatalogId { get; set; }

        public ServiceCatalog ServiceCatalog { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public InterceptionMode InterceptionMode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<InterceptedApi> InterceptedApis { get; set; }
    }
}
