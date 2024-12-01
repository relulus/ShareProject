using ShareProject.ApiPhantom.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShareProject.Entities
{
    public class ApiCatalog
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("ServiceCatalog")]
        public Guid ServiceCatalogId { get; set; }

        public ServiceCatalog ServiceCatalog { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public string Method { get; set; }

        public string Description { get; set; }
    }
}
