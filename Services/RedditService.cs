using HtmlAgilityPack;
using RedditAnalyzer.Models;

namespace RedditAnalyzer.Services;

public class RedditService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RedditService> _logger;

    public RedditService(HttpClient httpClient, ILogger<RedditService> logger)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; RedditAnalyzer/1.0)");
        _logger = logger;
    }

    public async Task<Dictionary<string, List<string>>> ProcessAsync(RequestModel request)
    {
        var result = new Dictionary<string, List<string>>();
        var tasks = request.Items.Select(item => ProcessSubreddit(item, request.Limit));
        var responses = await Task.WhenAll(tasks);
        foreach (var r in responses)
        {
            result[r.Key] = r.Value;
        }
        return result;
    }

    private async Task<KeyValuePair<string, List<string>>> ProcessSubreddit(SubredditItem item, int limit)
    {
        var url = $"https://old.reddit.com/{item.Subreddit}?limit={limit}";

        try
        {
            _logger.LogInformation("Fetching HTML for {Subreddit}", item.Subreddit);

            var html = await _httpClient.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var postNodes = doc.DocumentNode
                .SelectNodes("//div[contains(@class,'thing') and contains(@class,'link')]");

            if (postNodes == null)
            {
                _logger.LogWarning("No posts found in HTML for {Subreddit}", item.Subreddit);
                return new KeyValuePair<string, List<string>>($"/{item.Subreddit}", new List<string>());
            }

            var posts = new List<string>();

            foreach (var node in postNodes)
            {
                var titleNode = node.SelectSingleNode(".//a[contains(@class,'title')]");
                if (titleNode == null) continue;

                string title = titleNode.InnerText.Trim();
                string text = node.SelectSingleNode(".//div[contains(@class,'expando')]")?.InnerText.Trim() ?? "";

                if (item.Keywords.Any(k =>
                        title.ToLower().Contains(k.ToLower()) ||
                        text.ToLower().Contains(k.ToLower())))
                {
                    posts.Add(title);
                }
            }

            _logger.LogInformation("Done for {Subreddit}, found {Count} posts", item.Subreddit, posts.Count);
            return new KeyValuePair<string, List<string>>($"/{item.Subreddit}", posts);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error for {Subreddit}: {Error}", item.Subreddit, ex.Message);
            return new KeyValuePair<string, List<string>>($"/{item.Subreddit}",
                new List<string> { "ERROR: Cannot fetch subreddit" });
        }
    }
}