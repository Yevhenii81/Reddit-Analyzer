using HtmlAgilityPack;

namespace RedditAnalyzer.Services;

public class RedditHtmlParser
{
    public List<(string Title, string Text, bool HasImage)> ParsePosts(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var posts = new List<(string Title, string Text, bool HasImage)>();

        // Reddit old.reddit.com — стабильный HTML, не меняется
        var postNodes = doc.DocumentNode
            .SelectNodes("//div[contains(@class,'thing') and contains(@class,'link')]");

        if (postNodes == null) return posts;

        foreach (var node in postNodes)
        {
            var titleNode = node.SelectSingleNode(".//a[contains(@class,'title')]");
            if (titleNode == null) continue;

            string title = titleNode.InnerText.Trim();
            string text = node.SelectSingleNode(".//div[@class='expando']")?.InnerText.Trim() ?? "";
            bool hasImage = node.SelectSingleNode(".//img[@class='preview']") != null;

            posts.Add((title, text, hasImage));
        }

        return posts;
    }
}