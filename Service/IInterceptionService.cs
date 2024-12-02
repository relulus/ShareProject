using ApiPhantom.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiPhantom.Service.Interfaces
{
    public interface IInterceptionService
    {
        Task<IEnumerable<InterceptedService>> GetInterceptedServicesAsync();
        Task<IEnumerable<InterceptedApi>> GetInterceptedApisAsync(Guid interceptedServiceId);
        Task<(bool Success, string Message)> AddServiceForInterceptionAsync(Guid serviceCatalogId);
        Task<(bool Success, string Message)> AddApiForInterceptionAsync(Guid interceptedServiceId, Guid apiCatalogId);
    }
}
