using ApiPhantom.Models;
using System.Text.Json;

public class ApiPhantomService : IApiPhantomService
{
    private readonly ApiPhantomContext _context;
    private readonly HttpClient _httpClient;

    public ApiPhantomService(ApiPhantomContext context)
    {
        _context = context;
        _httpClient = new HttpClient();
    }

    public async Task<ProxyResponse> HandleRequestAsync(ProxyRequest proxyRequest)
    {
        // Match service and endpoint
        var service = await _context.Services
            .Include(s => s.ApiEndpoints)
            .FirstOrDefaultAsync(s => proxyRequest.Path.StartsWith(s.BaseUrl) && s.IsActive);

        if (service == null)
        {
            return null; // No matching service, let the next middleware handle it
        }

        var apiEndpoint = service.ApiEndpoints
            .FirstOrDefault(ae => ae.Method.Equals(proxyRequest.Method, StringComparison.OrdinalIgnoreCase) &&
                                  MatchPaths(ae.Path, proxyRequest.Path));

        if (apiEndpoint == null)
        {
            return null; // No matching endpoint, let the next middleware handle it
        }

        // Determine the InterceptionMode
        var interceptionMode = apiEndpoint.InterceptionMode ?? service.InterceptionMode;

        switch (interceptionMode)
        {
            case InterceptionMode.Block:
                return new ProxyResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Headers = new Dictionary<string, string>(),
                    Body = "Blocked by ApiPhantom."
                };

            case InterceptionMode.Replay:
                return await HandleReplayMode(apiEndpoint);

            case InterceptionMode.Intercept:
            case InterceptionMode.Live:
                return await HandleLiveOrInterceptMode(proxyRequest, service.BaseUrl, interceptionMode, apiEndpoint);
        }

        return null;
    }

    private async Task<ProxyResponse> HandleReplayMode(ApiEndpoint apiEndpoint)
    {
        var storedResponse = await _context.RequestResponses
            .Where(rr => rr.ApiEndpointId == apiEndpoint.Id)
            .OrderByDescending(rr => rr.Timestamp)
            .FirstOrDefaultAsync();

        if (storedResponse == null)
        {
            return new ProxyResponse
            {
                StatusCode = StatusCodes.Status404NotFound,
                Headers = new Dictionary<string, string>(),
                Body = "No stored response found."
            };
        }

        return new ProxyResponse
        {
            StatusCode = storedResponse.ResponseStatusCode,
            Headers = JsonSerializer.Deserialize<Dictionary<string, string>>(storedResponse.ResponseHeaders),
            Body = storedResponse.ResponseBody
        };
    }

    private async Task<ProxyResponse> HandleLiveOrInterceptMode(
        ProxyRequest proxyRequest, string baseUrl, InterceptionMode mode, ApiEndpoint apiEndpoint)
    {
        var targetUrl = $"{baseUrl}{proxyRequest.Path}{proxyRequest.QueryString}";
        var targetRequest = CreateHttpRequestMessage(proxyRequest, targetUrl);

        HttpResponseMessage targetResponse;
        try
        {
            targetResponse = await _httpClient.SendAsync(targetRequest);
        }
        catch (Exception ex)
        {
            return new ProxyResponse
            {
                StatusCode = StatusCodes.Status503ServiceUnavailable,
                Headers = new Dictionary<string, string>(),
                Body = $"Error forwarding request: {ex.Message}"
            };
        }

        var responseBody = await targetResponse.Content.ReadAsStringAsync();
        if (mode == InterceptionMode.Intercept)
        {
            await StoreRequestResponse(proxyRequest, apiEndpoint, responseBody, targetResponse);
        }

        return new ProxyResponse
        {
            StatusCode = (int)targetResponse.StatusCode,
            Headers = targetResponse.Headers.Concat(targetResponse.Content.Headers)
                .ToDictionary(h => h.Key, h => string.Join(",", h.Value)),
            Body = responseBody
        };
    }

    private HttpRequestMessage CreateHttpRequestMessage(ProxyRequest proxyRequest, string targetUrl)
    {
        var requestMessage = new HttpRequestMessage
        {
            Method = new HttpMethod(proxyRequest.Method),
            RequestUri = new Uri(targetUrl)
        };

        foreach (var header in proxyRequest.Headers)
        {
            requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (!string.IsNullOrEmpty(proxyRequest.Body))
        {
            requestMessage.Content = new StringContent(proxyRequest.Body);
        }

        return requestMessage;
    }

    private async Task StoreRequestResponse(ProxyRequest proxyRequest, ApiEndpoint apiEndpoint, string responseBody, HttpResponseMessage responseMessage)
    {
        var requestResponse = new RequestResponse
        {
            ApiEndpointId = apiEndpoint.Id,
            RequestMethod = proxyRequest.Method,
            RequestPath = proxyRequest.Path,
            RequestHeaders = JsonSerializer.Serialize(proxyRequest.Headers),
            RequestBody = proxyRequest.Body,
            ResponseStatusCode = (int)responseMessage.StatusCode,
            ResponseHeaders = JsonSerializer.Serialize(responseMessage.Headers.Concat(responseMessage.Content.Headers)
                .ToDictionary(h => h.Key, h => string.Join(",", h.Value))),
            ResponseBody = responseBody,
            Timestamp = DateTime.UtcNow
        };

        _context.RequestResponses.Add(requestResponse);
        await _context.SaveChangesAsync();
    }

    private bool MatchPaths(string templatePath, string requestPath)
    {
        return string.Equals(templatePath.TrimEnd('/'), requestPath.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
    }
}
