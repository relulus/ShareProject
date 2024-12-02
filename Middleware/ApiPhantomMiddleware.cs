using System.Net.Http;

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
        // Create a ProxyRequest from HttpContext
        var proxyRequest = await CreateProxyRequestAsync(context);

        // Pass the request to the service layer
        var response = await _apiPhantomService.HandleRequestAsync(proxyRequest);

        if (response != null)
        {
            // Write the response back to the HttpContext
            context.Response.StatusCode = response.StatusCode;
            foreach (var header in response.Headers)
            {
                context.Response.Headers[header.Key] = header.Value;
            }
            await context.Response.WriteAsync(response.Body);
        }
        else
        {
            // Pass to the next middleware if not handled
            await _next(context);
        }
    }

    private async Task<ProxyRequest> CreateProxyRequestAsync(HttpContext context)
    {
        var request = context.Request;

        // Read and buffer the body
        string body = null;
        if (request.ContentLength > 0)
        {
            context.Request.EnableBuffering();
            using (var reader = new StreamReader(request.Body, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset stream position
            }
        }

        return new ProxyRequest
        {
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString.Value,
            Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            Body = body
        };
    }
}
