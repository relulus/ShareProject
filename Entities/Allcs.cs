using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using System.Reflection.Emit;

namespace ApiPhantom.Models
{
    public enum InterceptionMode
    {
        Live,
        Intercept,
        Replay,
        Block
    }

    public enum HTTPMethod
    {
        GET,
        POST,
        PUT,
        DELETE,
        PATCH,
        OPTIONS,
        HEAD
    }

    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Username { get; set; }

        public string FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<InterceptedService> InterceptedServices { get; set; }
    }

    public class ServiceCatalog
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string BaseUrl { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(20)")]
        public InterceptionMode InterceptionMode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ApiCatalog> ApiCatalogs { get; set; }
    }

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
        public HTTPMethod Method { get; set; }

        public string MethodDescription { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public InterceptionMode? InterceptionMode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public InterceptedApi InterceptedApi { get; set; }
    }

    public class InterceptedService
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public User User { get; set; }

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

        public ICollection<InterceptedApi> InterceptedApis { get; set; }
    }

    public class InterceptedApi
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("InterceptedService")]
        public Guid InterceptedServiceId { get; set; }

        public InterceptedService InterceptedService { get; set; }

        [Required]
        [ForeignKey("ApiCatalog")]
        public Guid ApiCatalogId { get; set; }

        public ApiCatalog ApiCatalog { get; set; }

        [Column(TypeName = "nvarchar(20)")]
        public InterceptionMode? InterceptionMode { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<RequestResponse> RequestResponses { get; set; }
    }

    public class RequestResponse
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [ForeignKey("InterceptedApi")]
        public Guid InterceptedApiId { get; set; }

        public InterceptedApi InterceptedApi { get; set; }

        [Required]
        public string RequestMethod { get; set; }

        [Required]
        public string RequestPath { get; set; }

        public string RequestQueryParams { get; set; }

        public string RequestHeaders { get; set; }

        public string RequestBody { get; set; }

        public int ResponseStatusCode { get; set; }

        public string ResponseHeaders { get; set; }

        public string ResponseBody { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Dataset
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<DatasetRequestResponse> DatasetRequestResponses { get; set; }
    }

    public class DatasetRequestResponse
    {
        public Guid DatasetId { get; set; }

        public Dataset Dataset { get; set; }

        public Guid RequestResponseId { get; set; }

        public RequestResponse RequestResponse { get; set; }
    }

    public class ApiPhantomContext : DbContext
    {
        public ApiPhantomContext(DbContextOptions<ApiPhantomContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<ServiceCatalog> ServiceCatalogs { get; set; }

        public DbSet<ApiCatalog> ApiCatalogs { get; set; }

        public DbSet<InterceptedService> InterceptedServices { get; set; }

        public DbSet<InterceptedApi> InterceptedApis { get; set; }

        public DbSet<RequestResponse> RequestResponses { get; set; }

        public DbSet<Dataset> Datasets { get; set; }

        public DbSet<DatasetRequestResponse> DatasetRequestResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.InterceptedServices)
                .WithOne(is => is.User)
                .HasForeignKey(is => is.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceCatalog>()
                .HasMany(sc => sc.ApiCatalogs)
                .WithOne(ac => ac.ServiceCatalog)
                .HasForeignKey(ac => ac.ServiceCatalogId);

            modelBuilder.Entity<ApiCatalog>()
                .HasOne(ac => ac.InterceptedApi)
                .WithOne(ia => ia.ApiCatalog)
                .HasForeignKey<InterceptedApi>(ia => ia.ApiCatalogId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InterceptedService>()
                .HasMany(is => is.InterceptedApis)
                .WithOne(ia => ia.InterceptedService)
                .HasForeignKey(ia => ia.InterceptedServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InterceptedApi>()
                .HasMany(ia => ia.RequestResponses)
                .WithOne(rr => rr.InterceptedApi)
                .HasForeignKey(rr => rr.InterceptedApiId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Dataset>()
                .HasMany(d => d.DatasetRequestResponses)
                .WithOne(drr => drr.Dataset)
                .HasForeignKey(drr => drr.DatasetId);

            modelBuilder.Entity<DatasetRequestResponse>()
                .HasOne(drr => drr.RequestResponse)
                .WithMany(rr => rr.DatasetRequestResponses)
                .HasForeignKey(drr => drr.RequestResponseId);

            var dictionaryConverter = new ValueConverter<Dictionary<string, string>, string>(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null));

            modelBuilder.Entity<RequestResponse>()
                .Property(rr => rr.RequestQueryParams)
                .HasConversion(dictionaryConverter);

            modelBuilder.Entity<RequestResponse>()
                .Property(rr => rr.RequestHeaders)
                .HasConversion(dictionaryConverter);

            modelBuilder.Entity<RequestResponse>()
                .Property(rr => rr.ResponseHeaders)
                .HasConversion(dictionaryConverter);

            modelBuilder.Entity<RequestResponse>()
                .Property(rr => rr.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<RequestResponse>()
                .Property(rr => rr.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<RequestResponse>()
                .Property(rr => rr.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
