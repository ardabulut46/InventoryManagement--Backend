using InventoryManagement.Core.DTOs.ChatModel;
using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Infrastructure.Data;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class AppInfoController : ControllerBase
{
    private readonly OpenRouterService _openRouterService;
    private readonly DeepSeekService _deepSeekService;
    private readonly ApplicationDbContext _context;
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
        
    public AppInfoController(
        OpenRouterService openRouterService, 
        DeepSeekService deepSeekService,
        ApplicationDbContext context,
        Kernel kernel,
        IChatCompletionService chatService)
    {
        _openRouterService = openRouterService;
        _deepSeekService = deepSeekService;
        _context = context;
        _kernel = kernel;
        _chatService = chatService;
    }
    
    [HttpGet("ask")]
    public async Task<IActionResult> AskAboutApp([FromQuery] string question)
    {
        var answer = await _openRouterService.GetAppInfoAsync(question);
        return Ok(answer);
    }

    [HttpGet("ask-deepseek")]
    public async Task<IActionResult> AskDeepSeek([FromQuery] string question)
    {
        var answer = await _deepSeekService.GetAppInfoAsync(question);
        return Ok(answer);
    }
    
    [HttpGet("ollama")]
    public async Task<IActionResult> Ollama([FromQuery] string question)
    {
        try
        {
            var systemPrompt = @"You are an assistant for the Inventory Management System API. 
Answer questions about the application's functionality and features.
User will ask in Turkish. Translate technical terms: Ticket=Çağrı, Inventory=Envanter.
Only answer in Turkish.";

            var fullPrompt = $"{systemPrompt}\n\nUser Question: {question}";
            var result = await _kernel.InvokePromptAsync(fullPrompt);
            return Ok(new { answer = result.ToString() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatModelDto chatModel)
    {
        try 
        {
            // Get the chat message from the chat service
            var chatMessage = await _chatService.GetChatMessageContentAsync(chatModel.Input);
        
            // Return the content directly
            return Ok(new { answer = chatMessage.Content });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}