using System.Text;

public async Task<IActionResult> HandleLiveOrInterceptMode(HttpRequest originalRequest)
{
    var targetUri = new Uri("https://target-api.com" + originalRequest.Path + originalRequest.QueryString);

    var forwardedRequest = new HttpRequestMessage
    {
        RequestUri = targetUri,
        Method = new HttpMethod(originalRequest.Method)
    };

    // Copy headers
    foreach (var header in originalRequest.Headers)
    {
        if (!forwardedRequest.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
        {
            forwardedRequest.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    // Add body if applicable
    if (originalRequest.ContentLength > 0 && (originalRequest.Method == "POST" || originalRequest.Method == "PUT"))
    {
        using var reader = new StreamReader(originalRequest.Body);
        var body = await reader.ReadToEndAsync();
        forwardedRequest.Content = new StringContent(body, Encoding.UTF8, originalRequest.ContentType);
        originalRequest.Body.Position = 0; // Reset stream position
    }

    // Send request to target API
    try
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(forwardedRequest);

        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Target API Response: {response.StatusCode}, {responseBody}");

        return StatusCode((int)response.StatusCode, responseBody);
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Error forwarding request: {ex.Message}");
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            Message = "Error forwarding the request.",
            Details = ex.Message
        });
    }
}