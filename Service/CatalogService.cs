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
    public class CatalogService : ICatalogService
    {
        private readonly ApiPhantomContext _context;

        public CatalogService(ApiPhantomContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ServiceCatalog>> GetAllServicesAsync()
        {
            return await _context.ServiceCatalog
                .Select(s => new ServiceCatalog
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    BaseUrl = s.BaseUrl
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<ApiCatalog>> GetApisByServiceIdAsync(Guid serviceId)
        {
            var service = await _context.ServiceCatalog
                .Include(s => s.ApiCatalogs)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            return service?.ApiCatalogs ?? Enumerable.Empty<ApiCatalog>();
        }
    }
}
