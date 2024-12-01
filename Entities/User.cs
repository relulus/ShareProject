using System.ComponentModel.DataAnnotations;

namespace ShareProject.Entities
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Username { get; set; } 

        public string FullName { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<InterceptedService> InterceptedServices { get; set; }
    }
}
