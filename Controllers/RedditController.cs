using Microsoft.AspNetCore.Mvc;
using RedditAnalyzer.Models;
using RedditAnalyzer.Services;

namespace RedditAnalyzer.Controllers;

[ApiController]
[Route("api/reddit")]
public class RedditController : ControllerBase
{
    private readonly RedditService _service;

    public RedditController(RedditService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Analyze([FromBody] RequestModel request)
    {
        var result = await _service.ProcessAsync(request);
        return Ok(result);
    }
}