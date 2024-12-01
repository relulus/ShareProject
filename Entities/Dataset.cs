using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShareProject.Entities
{
    public class Dataset
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for many-to-many relationship
        public ICollection<DatasetRequestResponse> DatasetRequestResponses { get; set; }
    }
}
