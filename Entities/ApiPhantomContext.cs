﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Text.Json;
using ApiPhantom.Models;
using ShareProject.ApiPhantom.Models;
using ShareProject.Entities;
using System.Reflection.Emit;

namespace ApiPhantom.Data
{
    public class ApiPhantomContext : DbContext
    {
        public ApiPhantomContext(DbContextOptions<ApiPhantomContext> options)
            : base(options)
        {
        }

        // DbSet properties for all entities
        public DbSet<ServiceCatalog> ServiceCatalogs { get; set; }
        public DbSet<ApiCatalog> ApiCatalogs { get; set; }
        public DbSet<RequestResponse> RequestResponses { get; set; }
        public DbSet<InterceptedService> InterceptedServices { get; set; }
        public DbSet<InterceptedApi> InterceptedApis { get; set; }
        public DbSet<Dataset> Datasets { get; set; }
        public DbSet<DatasetRequestResponse> DatasetRequestResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure enum InterceptionMode to be stored as string
            modelBuilder.Entity<ServiceCatalog>()
                .Property(s => s.InterceptionMode)
                .HasConversion<string>();

            modelBuilder.Entity<ApiCatalog>()
                .Property(a => a.InterceptionMode)
                .HasConversion<string>();

            modelBuilder.Entity<InterceptedService>()
                .Property(is => is.InterceptionMode)
                .HasConversion<string>();

            modelBuilder.Entity<InterceptedApi>()
                .Property(ia => ia.InterceptionMode)
                .HasConversion<string>();

            // ServiceCatalog → ApiCatalog (One-to-Many)
            modelBuilder.Entity<ServiceCatalog>()
                .HasMany(sc => sc.ApiCatalogs)
                .WithOne(ac => ac.ServiceCatalog)
                .HasForeignKey(ac => ac.ServiceCatalogId);

            // ApiCatalog → RequestResponse (One-to-Many)
            modelBuilder.Entity<ApiCatalog>()
                .HasMany(ac => ac.RequestResponses)
                .WithOne(rr => rr.ApiCatalog)
                .HasForeignKey(rr => rr.ApiCatalogId);

            // ServiceCatalog → InterceptedService (One-to-One)
            modelBuilder.Entity<ServiceCatalog>()
                .HasOne(sc => sc.InterceptedService)
                .WithOne(is => is.ServiceCatalog)
                .HasForeignKey<InterceptedService>(is => is.ServiceCatalogId);

            // InterceptedService → InterceptedApi (One-to-Many)
            modelBuilder.Entity<InterceptedService>()
                .HasMany(is => is.InterceptedApis)
                .WithOne(ia => ia.InterceptedService)
                .HasForeignKey(ia => ia.InterceptedServiceId);

            // ApiCatalog → InterceptedApi (One-to-One)
            modelBuilder.Entity<ApiCatalog>()
                .HasOne(ac => ac.InterceptedApi)
                .WithOne(ia => ia.ApiCatalog)
                .HasForeignKey<InterceptedApi>(ia => ia.ApiCatalogId);

            // InterceptedApi → RequestResponse (One-to-Many)
            modelBuilder.Entity<InterceptedApi>()
                .HasMany(ia => ia.RequestResponses)
                .WithOne(rr => rr.InterceptedApi)
                .HasForeignKey(rr => rr.InterceptedApiId);

            // Dataset → DatasetRequestResponse (One-to-Many)
            modelBuilder.Entity<Dataset>()
                .HasMany(d => d.DatasetRequestResponses)
                .WithOne(drr => drr.Dataset)
                .HasForeignKey(drr => drr.DatasetId);

            // RequestResponse → DatasetRequestResponse (One-to-Many)
            modelBuilder.Entity<RequestResponse>()
                .HasMany(rr => rr.DatasetRequestResponses)
                .WithOne(drr => drr.RequestResponse)
                .HasForeignKey(drr => drr.RequestResponseId);

            // Configure JSON serialization for dictionary properties in RequestResponse
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

            // Set default values for IsActive and timestamps
            modelBuilder.Entity<ServiceCatalog>()
                .Property(s => s.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<ApiCatalog>()
                .Property(a => a.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<RequestResponse>()
                .Property(rr => rr.Timestamp)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<ServiceCatalog>()
                .Property(s => s.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<ServiceCatalog>()
                .Property(s => s.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<ApiCatalog>()
                .Property(a => a.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<ApiCatalog>()
                .Property(a => a.UpdatedAt)
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