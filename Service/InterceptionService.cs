using ApiPhantom.DAL;
using ApiPhantom.Models;
using ApiPhantom.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPhantom.Service
{
    public class InterceptionService : IInterceptionService
    {
        private readonly ApiPhantomContext _context;

        public InterceptionService(ApiPhantomContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<InterceptedService>> GetInterceptedServicesAsync()
        {
            return await _context.InterceptedService
                .Include(s => s.ServiceCatalog)
                .ToListAsync();
        }

        public async Task<IEnumerable<InterceptedApi>> GetInterceptedApisAsync(Guid interceptedServiceId)
        {
            var service = await _context.InterceptedService
                .Include(s => s.InterceptedApis)
                .ThenInclude(a => a.ApiCatalog)
                .FirstOrDefaultAsync(s => s.Id == interceptedServiceId);

            return service?.InterceptedApis ?? Enumerable.Empty<InterceptedApi>();
        }

        public async Task<(bool Success, string Message)> AddServiceForInterceptionAsync(Guid serviceCatalogId)
        {
            var serviceCatalog = await _context.ServiceCatalog.FindAsync(serviceCatalogId);
            if (serviceCatalog == null)
                return (false, "ServiceCatalog not found.");

            var interceptedService = new InterceptedService
            {
                Id = Guid.NewGuid(),
                ServiceCatalogId = serviceCatalog.Id,
                InterceptionMode = InterceptionMode.Live,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.InterceptedService.Add(interceptedService);
            await _context.SaveChangesAsync();

            return (true, "Service added for interception.");
        }

        public async Task<(bool Success, string Message)> AddApiForInterceptionAsync(Guid interceptedServiceId, Guid apiCatalogId)
        {
            var interceptedService = await _context.InterceptedService.FindAsync(interceptedServiceId);
            if (interceptedService == null)
                return (false, "InterceptedService not found.");

            var apiCatalog = await _context.ApiCatalog.FindAsync(apiCatalogId);
            if (apiCatalog == null)
                return (false, "ApiCatalog not found.");

            var interceptedApi = new InterceptedApi
            {
                Id = Guid.NewGuid(),
                InterceptedServiceId = interceptedService.Id,
                ApiCatalogId = apiCatalog.Id,
                InterceptionMode = InterceptionMode.Intercept,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.InterceptedApi.Add(interceptedApi);
            await _context.SaveChangesAsync();

            return (true, "API added for interception.");
        }
    }
}
