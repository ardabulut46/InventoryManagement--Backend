using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AppInfoController : ControllerBase
{
    private readonly OpenRouterService _openRouterService;
    private readonly DeepSeekService _deepSeekService;

    public AppInfoController(OpenRouterService openRouterService, DeepSeekService deepSeekService)
    {
        _openRouterService = openRouterService;
        _deepSeekService = deepSeekService;
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

public class AppInfoRequest
{
    public string Details { get; set; }
}