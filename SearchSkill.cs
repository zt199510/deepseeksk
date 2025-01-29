
using Newtonsoft.Json.Linq;

public class SearchResult
{
    public string Link { get; set; }
    public string Title { get; set; }
    public string Snippet { get; set; }
}

public class SearchSkill
{
    private readonly HttpClient _httpClient;

    public SearchSkill(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<SearchResult>> SearchAsync(
        string query,
        string queryUrl = "http://ÄãµÄSearchXNGµØÖ·",
        int count = 10,
        List<string> filterList = null,
        int pageNumber = 1,
        string language = "en-US",
        int safesearch = 1,
        string timeRange = "",
        List<string> categories = null)
    {
        try
        {
            var categoryString = categories != null ? string.Join(",", categories) : "";
            var parameters = new Dictionary<string, string>
            {
                { "q", query },
                { "format", "json" },
                { "pageno", pageNumber.ToString() },
                { "safesearch", safesearch.ToString() },
                { "language", language },
                { "time_range", timeRange },
                { "categories", categoryString },
                { "theme", "simple" },
                { "image_proxy", "0" }
            };

            var requestUri = $"{queryUrl}/?{string.Join("&", parameters.Select(pair => $"{pair.Key}={pair.Value}"))}";
            Console.WriteLine(requestUri);
            var response = await _httpClient.GetAsync(requestUri);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var jsonResults = JObject.Parse(jsonResponse)["results"] ?? new JArray();
            var results = jsonResults.ToObject<List<Dictionary<string, object>>>();

            var sortedResults = results.OrderByDescending(result => result.ContainsKey("score") ? Convert.ToDouble(result["score"]) : 0).ToList();

            if (filterList != null && filterList.Count > 0)
            {
                sortedResults = FilterResults(sortedResults, filterList);
            }

            return sortedResults.Take(count).Select(result => new SearchResult
            {
                Link = result.ContainsKey("url") ? result["url"].ToString() : "",
                Title = result.ContainsKey("title") ? result["title"].ToString() : null,
                Snippet = result.ContainsKey("content") ? result["content"].ToString() : null
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception("Search failed", ex);
        }
    }

    private List<Dictionary<string, object>> FilterResults(List<Dictionary<string, object>> results, List<string> filterList)
    {
        return results.Where(result =>
        {
            var url = result.ContainsKey("url") ? result["url"].ToString() : result.ContainsKey("link") ? result["link"].ToString() : "";
            var domain = new Uri(url).Host;
            return filterList.Any(filteredDomain => domain.EndsWith(filteredDomain));
        }).ToList();
    }
}