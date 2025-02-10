using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

public class DeepSeekService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IConfiguration _configuration;
    private readonly string _apiRoutes;

    public DeepSeekService(HttpClient httpClient, IConfiguration configuration, IEnumerable<EndpointDataSource> endpointDataSources)
    {
        _configuration = configuration;
        _apiKey = _configuration["OpenRouter:DeepSeekApiKey"];
        _apiRoutes = GetApiRoutes(endpointDataSources);
        
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new Exception("OpenRouter API key is not configured");
        }

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "localhost");
        _httpClient.DefaultRequestHeaders.Add("X-Title", "Inventory Management API");
    }

    private string GetApiRoutes(IEnumerable<EndpointDataSource> endpointSources)
    {
        var routes = endpointSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(endpoint => new
            {
                Method = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.FirstOrDefault(),
                Route = endpoint.RoutePattern.RawText,
                Controller = endpoint.Metadata.OfType<ControllerActionDescriptor>().FirstOrDefault()?.ControllerName
            })
            .Where(r => r.Method != null && r.Controller != null)
            .Select(r => $"{r.Method} {r.Route} -> {r.Controller}Controller")
            .OrderBy(r => r);
        return string.Join("\n", routes);
    }

    public async Task<string> GetAppInfoAsync(string userQuestion)
    {
        var requestBody = new
        {
            model = "deepseek/deepseek-r1:free",
            messages = new object[]
            {
                new {
                    role = "system",
                    content = $@"You are an assistant for the Inventory Management System API. Here are all the available API routes: {_apiRoutes}
Use this information to answer questions about the application's functionality and features. Give information about what the endpoints do, what is their purpose, do not answer like 'POST api/Ticket`'. 
When the user asks about the application, answer with the information you have from the scanned API routes and the code. User will ask in Turkish. So Ticket means Çağrı. Inventory means Envanter.
And the list goes on. So when the user asks about something from the app, if you can't find in Turkish in code, translate to English and scan again.
Only answer in Turkish."
                },
                new {
                    role = "user",
                    content = userQuestion
                }
            },
            temperature = 0.7,
            max_tokens = 2000
        };

        var jsonRequest = JsonSerializer.Serialize(requestBody);
        using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("chat/completions", content);
        if (!response.IsSuccessStatusCode)
        {
            var errorText = await response.Content.ReadAsStringAsync();
            throw new Exception($"OpenRouter API call failed. Status: {response.StatusCode}. Error: {errorText}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(responseJson);
        var root = document.RootElement;

        if (!root.TryGetProperty("choices", out JsonElement choices))
        {
            throw new Exception($"Expected 'choices' property was not found in the API response. Response: {responseJson}");
        }

        if (choices.GetArrayLength() == 0)
        {
            throw new Exception("No choices found in the API response.");
        }

        var firstChoice = choices[0];

        if (!firstChoice.TryGetProperty("message", out JsonElement messageElement))
        {
            throw new Exception("Expected 'message' property was not found in the first choice.");
        }

        if (!messageElement.TryGetProperty("content", out JsonElement contentElement))
        {
            throw new Exception("Expected 'content' property was not found in the message element.");
        }

        return contentElement.GetString();
    }
}