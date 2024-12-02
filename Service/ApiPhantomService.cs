using Microsoft.AspNetCore.Http;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ApiPhantom.Models;

namespace Service.Implementations
{
    public class ApiPhantomService : IApiPhantomService
    {
        private readonly ApiPhantomContext _context;
        private readonly HttpClient _httpClient;

        public ApiPhantomService(ApiPhantomContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        public async Task<bool> HandleRequestAsync(HttpContext context)
        {
            var request = context.Request;
            var method = request.Method;
            var path = request.Path.Value;
            var queryString = request.QueryString.Value;

            // Match service and endpoint
            var service = await _context.Services
                .Include(s => s.ApiEndpoints)
                .FirstOrDefaultAsync(s => path.StartsWith(s.BaseUrl) && s.IsActive);

            if (service == null)
            {
                return false; // No matching service, let the next middleware handle it
            }

            var apiEndpoint = service.ApiEndpoints
                .FirstOrDefault(ae => ae.Method.Equals(method, StringComparison.OrdinalIgnoreCase) && MatchPaths(ae.Path, path));

            if (apiEndpoint == null)
            {
                return false; // No matching endpoint, let the next middleware handle it
            }

            // Determine the InterceptionMode
            var interceptionMode = apiEndpoint.InterceptionMode ?? service.InterceptionMode;

            switch (interceptionMode)
            {
                case InterceptionMode.Block:
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Blocked by ApiPhantom.");
                    return true;

                case InterceptionMode.Replay:
                    return await HandleReplayMode(context, apiEndpoint);

                case InterceptionMode.Intercept:
                case InterceptionMode.Live:
                    return await HandleLiveOrInterceptMode(context, service.BaseUrl, path, queryString, interceptionMode, apiEndpoint);
            }

            return false; // Default to letting the next middleware handle it
        }

        private async Task<bool> HandleReplayMode(HttpContext context, ApiEndpoint apiEndpoint)
        {
            var storedResponse = await _context.RequestResponses
                .Where(rr => rr.ApiEndpointId == apiEndpoint.Id)
                .OrderByDescending(rr => rr.Timestamp)
                .FirstOrDefaultAsync();

            if (storedResponse == null)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync("No stored response found.");
                return true;
            }

            context.Response.StatusCode = storedResponse.ResponseStatusCode;
            foreach (var header in JsonSerializer.Deserialize<Dictionary<string, string>>(storedResponse.ResponseHeaders))
            {
                context.Response.Headers[header.Key] = header.Value;
            }

            await context.Response.WriteAsync(storedResponse.ResponseBody);
            return true;
        }

        private async Task<bool> HandleLiveOrInterceptMode(
            HttpContext context, string baseUrl, string path, string queryString,
            InterceptionMode mode, ApiEndpoint apiEndpoint)
        {
            var targetUrl = $"{baseUrl}{path}{queryString}";
            var targetRequest = CreateHttpRequestMessage(context, targetUrl);

            HttpResponseMessage targetResponse;
            try
            {
                targetResponse = await _httpClient.SendAsync(targetRequest);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync($"Error forwarding request: {ex.Message}");
                return true;
            }

            context.Response.StatusCode = (int)targetResponse.StatusCode;
            foreach (var header in targetResponse.Headers.Concat(targetResponse.Content.Headers))
            {
                context.Response.Headers[header.Key] = string.Join(",", header.Value);
            }

            var responseBody = await targetResponse.Content.ReadAsStringAsync();
            await context.Response.WriteAsync(responseBody);

            if (mode == InterceptionMode.Intercept)
            {
                await StoreRequestResponse(context, apiEndpoint, responseBody, targetResponse);
            }

            return true;
        }

        private HttpRequestMessage CreateHttpRequestMessage(HttpContext context, string targetUrl)
        {
            var requestMessage = new HttpRequestMessage
            {
                Method = new HttpMethod(context.Request.Method),
                RequestUri = new Uri(targetUrl)
            };

            foreach (var header in context.Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            return requestMessage;
        }

        private async Task StoreRequestResponse(HttpContext context, ApiEndpoint apiEndpoint, string responseBody, HttpResponseMessage responseMessage)
        {
            var requestBody = await ReadRequestBodyAsync(context.Request);

            var requestResponse = new RequestResponse
            {
                ApiEndpointId = apiEndpoint.Id,
                RequestMethod = context.Request.Method,
                RequestPath = context.Request.Path,
                ResponseBody = responseBody,
                ResponseStatusCode = (int)responseMessage.StatusCode,
                Timestamp = DateTime.UtcNow
            };

            _context.RequestResponses.Add(requestResponse);
            await _context.SaveChangesAsync();
        }

        private async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            var body = await new StreamReader(request.Body).ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }

        private bool MatchPaths(string templatePath, string requestPath)
        {
            return string.Equals(templatePath.TrimEnd('/'), requestPath.TrimEnd('/'), StringComparison.OrdinalIgnoreCase);
        }
    }
}
