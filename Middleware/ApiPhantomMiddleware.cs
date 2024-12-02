using Microsoft.AspNetCore.Http;
using Service.Implementations;
using Service.Interfaces;
using System.Net.Http;
using System.Threading.Tasks;

namespace Web
{
    public class ApiPhantomMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApiPhantomService _apiPhantomService;

        public ApiPhantomMiddleware(RequestDelegate next, IApiPhantomService apiPhantomService)
        {
            _next = next;
            _apiPhantomService = apiPhantomService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Pass the HttpContext to the service layer for handling
            if (!await _apiPhantomService.HandleRequestAsync(context))
            {
                // If the service layer does not handle the request, pass it to the next middleware
                await _next(context);
            }
        }
    }
}
