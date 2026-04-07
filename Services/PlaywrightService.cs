using Microsoft.Playwright;
using RedditAnalyzer.Models;

namespace RedditAnalyzer.Services;

public class PlaywrightService
{
    private readonly ILogger<PlaywrightService> _logger;

    public PlaywrightService(ILogger<PlaywrightService> logger)
    {
        _logger = logger;
    }

    public async Task<Dictionary<string, List<string>>> ProcessAsync(RequestModel request)
    {
        var tasks = request.Items.Select(item => ProcessSubreddit(item, request.Limit));
        var responses = await Task.WhenAll(tasks);
        return responses.ToDictionary(r => r.Key, r => r.Value);
    }

    private async Task<KeyValuePair<string, List<string>>> ProcessSubreddit(SubredditItem item, int limit)
    {
        var key = $"/{item.Subreddit}";
        var url = $"https://www.reddit.com/{item.Subreddit}/?limit={limit}";

        try
        {
            _logger.LogInformation("Playwright NEW Reddit → {Subreddit}", item.Subreddit);

            var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                SlowMo = 250,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-blink-features=AutomationControlled" }
            });

            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36",
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                Locale = "en-US",
                TimezoneId = "Europe/Kiev"
            });

            var page = await context.NewPageAsync();

            await page.AddInitScriptAsync("""
                Object.defineProperty(navigator, 'webdriver', {get: () => undefined});
                Object.defineProperty(navigator, 'plugins', {get: () => [1,2,3,4,5]});
                window.chrome = { runtime: {} };
            """);

            await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });

            await page.WaitForTimeoutAsync(3000);
            await page.EvaluateAsync("window.scrollBy(0, 1800)");
            await page.WaitForTimeoutAsync(2200);
            await page.EvaluateAsync("window.scrollBy(0, 1200)");
            await page.WaitForTimeoutAsync(1800);

            const string postSelector = "shreddit-post";
            await page.WaitForSelectorAsync(postSelector, new PageWaitForSelectorOptions { Timeout = 40000 });

            var posts = new List<string>();
            var postNodes = await page.QuerySelectorAllAsync(postSelector);

            _logger.LogInformation("Найдены {Count} shreddit-post", postNodes.Count);

            foreach (var node in postNodes.Take(limit))
            {
                var titleEl = await node.QuerySelectorAsync("h3, shreddit-title, [slot='title']");
                string title = titleEl != null ? (await titleEl.InnerTextAsync()).Trim() : "";

                var textEl = await node.QuerySelectorAsync("div.text, [data-click-id='text']");
                string text = textEl != null ? (await textEl.InnerTextAsync()).Trim() : "";

                if (!string.IsNullOrEmpty(title) &&
                    item.Keywords.Any(k => title.ToLower().Contains(k.ToLower()) || text.ToLower().Contains(k.ToLower())))
                {
                    posts.Add(title);
                }
            }

            _logger.LogInformation("NEW Reddit {Subreddit} → {Count} постів", item.Subreddit, posts.Count);
            return new KeyValuePair<string, List<string>>(key, posts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Playwright NEW Reddit error {Subreddit}", item.Subreddit);
            return new KeyValuePair<string, List<string>>(key, new List<string> { "ERROR: Reddit blocked" });
        }
    }
}