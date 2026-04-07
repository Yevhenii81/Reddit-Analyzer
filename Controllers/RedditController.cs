using Microsoft.AspNetCore.Mvc;
using RedditAnalyzer.Models;
using RedditAnalyzer.Services;

namespace RedditAnalyzer.Controllers;

[ApiController]
[Route("api/reddit")]
public class RedditController : ControllerBase
{
    private readonly RedditService _service;
    private readonly PlaywrightService _playwrightService;

    public RedditController(RedditService service, PlaywrightService playwrightService)
    {
        _service = service;
        _playwrightService = playwrightService;
    }

    [HttpPost]
    public async Task<IActionResult> Analyze([FromBody] RequestModel request)
    {
        var result = await _service.ProcessAsync(request);
        return Ok(result);
    }

    [HttpPost("playwright")]
    public async Task<IActionResult> AnalyzeWithPlaywright([FromBody] RequestModel request)
    {
        var result = await _playwrightService.ProcessAsync(request);
        return Ok(result);
    }
}