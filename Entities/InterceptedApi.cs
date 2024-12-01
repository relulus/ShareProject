using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareProject.Entities
{
    public class InterceptedApi
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("ApiCatalog")]
        public Guid ApiCatalogId { get; set; }

        public ApiCatalog ApiCatalog { get; set; }

        [Required]
        [ForeignKey("InterceptedService")]
        public Guid InterceptedServiceId { get; set; }

        public InterceptedService InterceptedService { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public InterceptionMode? InterceptionMode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<RequestResponse> RequestResponses { get; set; }
    }
}