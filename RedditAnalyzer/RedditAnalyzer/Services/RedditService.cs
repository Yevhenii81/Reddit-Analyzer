using Newtonsoft.Json;
using RedditAnalyzer.Models;

namespace RedditAnalyzer.Services;

public class RedditService
{
    private readonly HttpClient _httpClient;

    public RedditService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("TestApp");
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
        var url = $"https://www.reddit.com/{item.Subreddit}.json?limit={limit}";

        try
        {
            var json = await _httpClient.GetStringAsync(url);
            dynamic data = JsonConvert.DeserializeObject(json);

            var posts = new List<string>();

            foreach (var post in data.data.children)
            {
                string title = post.data.title;
                string text = post.data.selftext;

                if (item.Keywords.Any(k =>
                        title.ToLower().Contains(k.ToLower()) ||
                        text.ToLower().Contains(k.ToLower())))
                {
                    posts.Add(title);
                }
            }

            return new KeyValuePair<string, List<string>>($"/{item.Subreddit}", posts);
        }
        catch
        {
            return new KeyValuePair<string, List<string>>($"/{item.Subreddit}",
                new List<string> { "ERROR: Cannot fetch subreddit" });
        }
    }
}