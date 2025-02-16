using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using InventoryManagement.Infrastructure.Data;
using InventoryManagement.Infrastructure.Services;

public class OpenRouterService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly IConfiguration _configuration;
    private readonly string _apiRoutes;
    private readonly ApplicationDbContext _context;
    private readonly DynamicQueryService _dynamicQueryService;

    public OpenRouterService(
        HttpClient httpClient, 
        IConfiguration configuration,
        IEnumerable<EndpointDataSource> endpointDataSources,
        ApplicationDbContext context,
        DynamicQueryService dynamicQueryService)
    {
        _configuration = configuration;
        _apiKey = _configuration["OpenRouter:ApiKey"];
        _apiRoutes = GetApiRoutes(endpointDataSources);
        _context = context;
        _dynamicQueryService = dynamicQueryService;

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

    private async Task<string> AnalyzeQuestionForDynamicData(string question)
    {
        var questionLower = question.ToLower();
        
        // Turkish keywords mapping
        if (questionLower.Contains("kaç") || questionLower.Contains("sayısı") || 
            questionLower.Contains("toplam") || questionLower.Contains("mevcut"))
        {
            if (questionLower.Contains("envanter") || questionLower.Contains("inventory"))
                return "total_inventories";
            if (questionLower.Contains("çağrı") || questionLower.Contains("ticket"))
                return "total_tickets";
            if (questionLower.Contains("kullanıcı") || questionLower.Contains("user"))
                return "total_users";
            if (questionLower.Contains("garanti") && 
                (questionLower.Contains("bit") || questionLower.Contains("geç")))
                return "warranty_expired";
            if (questionLower.Contains("aktif") || questionLower.Contains("active"))
                return "active_inventories";
            if (questionLower.Contains("açık") && 
                (questionLower.Contains("çağrı") || questionLower.Contains("ticket")))
                return "open_tickets";
        }

        if (questionLower.Contains("en çok") || questionLower.Contains("popüler"))
        {
            if (questionLower.Contains("envanter") || questionLower.Contains("inventory"))
                return "most_common_inventory_type";
        }

        return null;
    }

    public async Task<string> GetAppInfoAsync(string userQuestion)
    {
        string dynamicData = null;
        
        try
        {
            var queryType = await AnalyzeQuestionForDynamicData(userQuestion);
            if (queryType != null)
            {
                var result = await _dynamicQueryService.ExecuteQuery(queryType);
                dynamicData = JsonSerializer.Serialize(result);
            }
        }
        catch (Exception ex)
        {
            // Log the error but continue with the normal response
            Console.WriteLine($"Dynamic query failed: {ex.Message}");
        }

        var requestBody = new
        {
            model = "google/gemini-2.0-flash-lite-preview-02-05:free",
            messages = new object[]
            {
                new {
                    role = "system",
                    content = $@"You are an assistant for the Inventory Management System API. 
Here are all the available API routes: {_apiRoutes}

Real-time system data: {dynamicData}

Use this information to answer questions about the application's functionality and features. 
When providing statistics or counts, use the real-time data provided above.
User will ask in Turkish. Translate technical terms: Ticket=Çağrı, Inventory=Envanter.
Only answer in Turkish. When giving numbers from real-time data, format them appropriately in Turkish."
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
        var choices = root.GetProperty("choices");
        if (choices.GetArrayLength() > 0)
        {
            var firstMessage = choices[0].GetProperty("message");
            var appInfo = firstMessage.GetProperty("content").GetString();
            return appInfo;
        }

        return string.Empty;
    }
}