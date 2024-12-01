namespace ShareProject
{
    using global::ApiPhantom.Models;
    using ShareProject.Entities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace ApiPhantom.Models
    {
        public class ServiceCatalog
        {
            [Key]
            public Guid Id { get; set; }

            [Required]
            public string Name { get; set; }

            public string Description { get; set; }

            [Required]
            public string BaseUrl { get; set; }

            // New fields
            [Required]
            [Column(TypeName = "nvarchar(20)")]
            public InterceptionMode InterceptionMode { get; set; } = InterceptionMode.Live;

            public bool IsActive { get; set; } = true;

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

            // Navigation property
            public ICollection<ApiCatalog> ApiCatalogs { get; set; }
        }
    }

}