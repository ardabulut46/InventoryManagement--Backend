using Microsoft.AspNetCore.Mvc;
using InventoryManagement.Infrastructure.Data;

[ApiController]
[Route("api/[controller]")]
public class AppInfoController : ControllerBase
{
    private readonly OpenRouterService _openRouterService;
    private readonly DeepSeekService _deepSeekService;
    private readonly ApplicationDbContext _context;

    public AppInfoController(
        OpenRouterService openRouterService, 
        DeepSeekService deepSeekService,
        ApplicationDbContext context)
    {
        _openRouterService = openRouterService;
        _deepSeekService = deepSeekService;
        _context = context;
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
}