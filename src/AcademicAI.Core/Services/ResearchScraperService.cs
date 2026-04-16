using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using AcademicAI.Core.Models;

namespace AcademicAI.Core.Services;

/// <summary>
/// Scrapes real search results from academic source websites:
/// Anna's Archive, LibGen, Z-Library.
/// </summary>
public class ResearchScraperService
{
    private static readonly HttpClient _http;

    static ResearchScraperService()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            AllowAutoRedirect = true,
        };
        _http = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
        _http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
        _http.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        _http.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
    }

    /// <summary>
    /// Search across multiple sources and merge results.
    /// </summary>
    public async Task<List<SearchResult>> SearchAllAsync(string query, CancellationToken ct = default)
    {
        var results = new List<SearchResult>();

        // Run searches in parallel
        var tasks = new[]
        {
            SearchAnnasArchiveAsync(query, ct),
            SearchLibGenAsync(query, ct),
        };

        try
        {
            await Task.WhenAll(tasks);
        }
        catch { }

        foreach (var task in tasks)
        {
            if (task.IsCompletedSuccessfully && task.Result != null)
                results.AddRange(task.Result);
        }

        return results;
    }

    /// <summary>
    /// Scrape search results from Anna's Archive.
    /// </summary>
    public async Task<List<SearchResult>> SearchAnnasArchiveAsync(string query, CancellationToken ct = default)
    {
        var results = new List<SearchResult>();
        try
        {
            var url = $"https://annas-archive.gl/search?q={Uri.EscapeDataString(query)}";
            var html = await _http.GetStringAsync(url, ct);

            // Anna's Archive renders search results in divs — parse them
            // Look for result entries: each has a title link, author, and metadata
            var partials = Regex.Matches(html,
                @"<a[^>]*href=""(/md5/[^""]+)""[^>]*>.*?</a>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            // Better approach: find the content blocks
            var blocks = Regex.Matches(html,
                @"<a\s+[^>]*href=""(/md5/[0-9a-fA-F]+)""[^>]*class=""[^""]*""[^>]*>(.*?)</a>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match block in blocks)
            {
                if (results.Count >= 15) break;
                var relUrl = block.Groups[1].Value;
                var inner = block.Groups[2].Value;

                // Extract title — usually the most prominent text
                var titleMatch = Regex.Match(inner,
                    @"<h3[^>]*>(.*?)</h3>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                var title = titleMatch.Success ? StripHtml(titleMatch.Groups[1].Value).Trim() : "";

                if (string.IsNullOrWhiteSpace(title))
                {
                    // Fallback: try to get text content
                    title = StripHtml(inner).Trim();
                    if (title.Length > 200) title = title[..200];
                }

                if (string.IsNullOrWhiteSpace(title) || title.Length < 3) continue;

                // Try to extract author and other metadata
                var authorMatch = Regex.Match(inner,
                    @"(?:by|author)[:\s]*([^<]+)", RegexOptions.IgnoreCase);
                var authors = authorMatch.Success ? authorMatch.Groups[1].Value.Trim() : "";

                // Try to find year
                var yearMatch = Regex.Match(inner, @"\b(19|20)\d{2}\b");
                var year = yearMatch.Success ? yearMatch.Value : "";

                // Try to find file info
                var fileMatch = Regex.Match(inner,
                    @"(pdf|epub|djvu|mobi)", RegexOptions.IgnoreCase);
                var fileType = fileMatch.Success ? fileMatch.Value.ToUpper() : "";

                var sizeMatch = Regex.Match(inner,
                    @"(\d+(?:\.\d+)?\s*[KMG]B)", RegexOptions.IgnoreCase);
                var fileSize = sizeMatch.Success ? sizeMatch.Value : "";

                results.Add(new SearchResult
                {
                    Title = title,
                    Authors = authors,
                    Year = year,
                    Abstract = $"Found on Anna's Archive" + (fileType != "" ? $" • {fileType}" : "") + (fileSize != "" ? $" • {fileSize}" : ""),
                    Url = $"https://annas-archive.gl{relUrl}",
                    Source = "Anna's Archive",
                    FileType = fileType,
                    FileSize = fileSize,
                    Citation = $"{authors}{(authors != "" ? ". " : "")}{title}{(year != "" ? $" ({year})" : "")}",
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Anna's Archive scrape error: {ex.Message}");
        }
        return results;
    }

    /// <summary>
    /// Scrape search results from Library Genesis.
    /// </summary>
    public async Task<List<SearchResult>> SearchLibGenAsync(string query, CancellationToken ct = default)
    {
        var results = new List<SearchResult>();
        try
        {
            var url = $"https://libgen.li/index.php?req={Uri.EscapeDataString(query)}&lg_topic=libgen&open=0&view=simple&res=25&phrase=1&column=def";
            var html = await _http.GetStringAsync(url, ct);

            // LibGen renders results in a table. Find table rows.
            var tableMatch = Regex.Match(html,
                @"<table[^>]*class=""c""[^>]*>(.*?)</table>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            if (!tableMatch.Success)
            {
                // Try alternate table pattern
                tableMatch = Regex.Match(html,
                    @"<table[^>]*>(.*?)</table>",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);
            }

            // Parse rows — skip header row
            var rows = Regex.Matches(html,
                @"<tr[^>]*>(.*?)</tr>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match row in rows)
            {
                if (results.Count >= 15) break;
                var rowHtml = row.Groups[1].Value;

                // Skip rows without enough columns (header, etc.)
                var cells = Regex.Matches(rowHtml,
                    @"<td[^>]*>(.*?)</td>",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (cells.Count < 3) continue;

                // Skip header rows
                if (rowHtml.Contains("<th", StringComparison.OrdinalIgnoreCase)) continue;

                // LibGen columns: ID, Author(s), Title, Publisher, Year, Pages, Language, Size, Extension, Mirror
                // But column order varies — find the title link
                var titleLink = Regex.Match(rowHtml,
                    @"<a[^>]*href=""([^""]*book/index\.php[^""]*)""[^>]*>(.*?)</a>",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (!titleLink.Success)
                {
                    // Alternate: just find any title-like link
                    titleLink = Regex.Match(rowHtml,
                        @"<a[^>]*>((?:(?!</a>).){10,})</a>",
                        RegexOptions.Singleline | RegexOptions.IgnoreCase);
                }

                if (!titleLink.Success) continue;

                var title = StripHtml(titleLink.Groups[2].Success ? titleLink.Groups[2].Value : titleLink.Groups[1].Value).Trim();
                if (title.Length < 3) continue;

                // Extract author from cells (usually first or second column with text)
                var authors = "";
                var year = "";
                var fileSize = "";
                var fileType = "";

                foreach (Match cell in cells)
                {
                    var cellText = StripHtml(cell.Groups[1].Value).Trim();

                    if (string.IsNullOrWhiteSpace(cellText)) continue;

                    // Year detection
                    if (Regex.IsMatch(cellText, @"^(19|20)\d{2}$"))
                    {
                        year = cellText;
                        continue;
                    }

                    // File extension
                    if (Regex.IsMatch(cellText, @"^(pdf|epub|djvu|mobi|azw3|doc|txt)$", RegexOptions.IgnoreCase))
                    {
                        fileType = cellText.ToUpper();
                        continue;
                    }

                    // File size
                    if (Regex.IsMatch(cellText, @"^\d+.*[KMG]b$", RegexOptions.IgnoreCase))
                    {
                        fileSize = cellText;
                        continue;
                    }

                    // Author guess: if not the title and looks like a name
                    if (authors == "" && cellText != title && cellText.Length > 2 && cellText.Length < 200
                        && !cellText.Contains("http") && !cellText.All(char.IsDigit))
                    {
                        authors = cellText;
                    }
                }

                var detailUrl = titleLink.Groups[1].Success && titleLink.Groups[1].Value.Contains("book")
                    ? $"https://libgen.li/{titleLink.Groups[1].Value}"
                    : "";

                results.Add(new SearchResult
                {
                    Title = title,
                    Authors = authors,
                    Year = year,
                    Abstract = $"Found on Library Genesis" + (fileType != "" ? $" • {fileType}" : "") + (fileSize != "" ? $" • {fileSize}" : ""),
                    Url = detailUrl,
                    Source = "LibGen",
                    FileType = fileType,
                    FileSize = fileSize,
                    Citation = $"{authors}{(authors != "" ? ". " : "")}{title}{(year != "" ? $" ({year})" : "")}",
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LibGen scrape error: {ex.Message}");
        }
        return results;
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return "";
        var text = Regex.Replace(html, @"<[^>]+>", " ");
        text = WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\s+", " ");
        return text.Trim();
    }
}
