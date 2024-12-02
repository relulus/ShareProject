using ApiPhantom.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiPhantom.Service.Interfaces
{
    public interface ICatalogService
    {
        Task<IEnumerable<ServiceCatalog>> GetAllServicesAsync();
        Task<IEnumerable<ApiCatalog>> GetApisByServiceIdAsync(Guid serviceId);
    }
}
