using ShareProject.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPhantom.Models
{
    public class ApiEndpoint
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("Service")]
        public Guid ServiceId { get; set; }

        public Service Service { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public string Method { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public InterceptionMode? InterceptionMode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<RequestResponse> RequestResponses { get; set; }
    }
}
