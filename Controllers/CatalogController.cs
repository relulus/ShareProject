using ApiPhantom.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ApiPhantom.Web
{
    [Route("api/catalog-services")]
    [ApiController]
    public class CatalogServicesController : ControllerBase
    {
        private readonly ICatalogService _catalogService;

        public CatalogServicesController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCatalogServices()
        {
            var services = await _catalogService.GetAllServicesAsync();
            return Ok(services);
        }

        [HttpGet("{id}/apis")]
        public async Task<IActionResult> GetApisByServiceId(Guid id)
        {
            var apis = await _catalogService.GetApisByServiceIdAsync(id);
            if (apis == null || !apis.Any())
                return NotFound(new { Message = "No APIs found for this service." });

            return Ok(apis);
        }
    }
}
