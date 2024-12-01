using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Text.Json;

namespace ShareProject.Entities
{
    public class RequestResponse
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("ApiEndpoint")]
        public Guid ApiEndpointId { get; set; }

        public ApiEndpoint ApiEndpoint { get; set; }

        [Required]
        public string RequestMethod { get; set; }

        [Required]
        public string RequestPath { get; set; }

        // JSON serialized dictionary
        public string RequestQueryParams { get; set; }

        // JSON serialized dictionary
        public string RequestHeaders { get; set; }

        public string RequestBody { get; set; }

        public int ResponseStatusCode { get; set; }

        // JSON serialized dictionary
        public string ResponseHeaders { get; set; }

        public string ResponseBody { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for many-to-many relationship
        public ICollection<DatasetRequestResponse> DatasetRequestResponses { get; set; }
    }
}
