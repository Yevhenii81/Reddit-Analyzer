namespace RedditAnalyzer.Models;

public class RequestModel
{
    public List<SubredditItem> Items { get; set; }
    public int Limit { get; set; }
}

public class SubredditItem
{
    public string Subreddit { get; set; }
    public List<string> Keywords { get; set; }
}