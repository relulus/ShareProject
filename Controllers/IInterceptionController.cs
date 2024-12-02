using ApiPhantom.Service.Interfaces;
using ApiPhantom.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ApiPhantom.Web.Controllers
{
    [Route("api/v1/interception")]
    [ApiController]
    public class InterceptionController : ControllerBase
    {
        private readonly IInterceptionService _interceptionService;

        public InterceptionController(IInterceptionService interceptionService)
        {
            _interceptionService = interceptionService;
        }

        [HttpGet("services")]
        public async Task<IActionResult> GetInterceptedServices()
        {
            var services = await _interceptionService.GetInterceptedServicesAsync();
            return Ok(services);
        }

        [HttpGet("services/{id}/apis")]
        public async Task<IActionResult> GetInterceptedApis(Guid id)
        {
            var apis = await _interceptionService.GetInterceptedApisAsync(id);
            if (apis == null || !apis.Any())
                return NotFound(new { Message = "No intercepted APIs found for this service." });

            return Ok(apis);
        }

        [HttpPost("services")]
        public async Task<IActionResult> AddServiceForInterception([FromBody] Guid serviceCatalogId)
        {
            var result = await _interceptionService.AddServiceForInterceptionAsync(serviceCatalogId);
            if (!result.Success)
                return BadRequest(new { result.Message });

            return Ok(new { Message = "Service added for interception." });
        }

        [HttpPost("services/{id}/apis")]
        public async Task<IActionResult> AddApiForInterception(Guid id, [FromBody] Guid apiCatalogId)
        {
            var result = await _interceptionService.AddApiForInterceptionAsync(id, apiCatalogId);
            if (!result.Success)
                return BadRequest(new { result.Message });

            return Ok(new { Message = "API added for interception." });
        }
    }
}
