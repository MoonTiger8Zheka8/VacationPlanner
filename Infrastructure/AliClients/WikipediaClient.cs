using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace VacationPlanner.Infrastructure.ApiClients;

public class WikipediaClient
{
    private readonly HttpClient _http;

    public WikipediaClient(HttpClient http)
    {
        _http = http;

        _http.DefaultRequestHeaders.UserAgent.Clear();
        _http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("VacationPlannerApp", "1.0"));
        _http.DefaultRequestHeaders.Accept.Clear();
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// 
    /// URL thumbnail з Wikipedia.
    /// 
    /// 
    public async Task<string?> TryGetImageAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return null;

        // пошук сторінки
        var pageId = await SearchPageIdAsync(query);
        if (pageId == null)
        {
            // fallback - без лапок/спецсимволів
            var simplified = SimplifyQuery(query);
            if (simplified != query)
                pageId = await SearchPageIdAsync(simplified);
        }

        if (pageId == null)
            return null;

        //thumbnail по pageid
        return await GetImageByPageIdAsync(pageId.Value);
    }

    private async Task<long?> SearchPageIdAsync(string query)
    {
        var url =
            $"https://uk.wikipedia.org/w/api.php?action=query&format=json&list=search" +
            $"&srsearch={Uri.EscapeDataString(query)}&srlimit=1";

        try
        {
            var json = await _http.GetFromJsonAsync<SearchResponse>(url);
            var item = json?.query?.search?.FirstOrDefault();
            return item?.pageid;
        }
        catch
        {
            return null;
        }
    }

    private async Task<string?> GetImageByPageIdAsync(long pageId)
    {
        var url =
            $"https://uk.wikipedia.org/w/api.php?action=query&format=json&prop=pageimages" +
            $"&piprop=thumbnail&pithumbsize=800&pageids={pageId}";

        try
        {
            var json = await _http.GetFromJsonAsync<PageImagesResponse>(url);
            if (json?.query?.pages == null) return null;

            var page = json.query.pages.Values.FirstOrDefault();
            return page?.thumbnail?.source;
        }
        catch
        {
            return null;
        }
    }

    private static string SimplifyQuery(string q)
    {
        // прибираємо зайве
        return q.Replace("\"", "").Replace("«", "").Replace("»", "").Trim();
    }

    // DTO
    public class SearchResponse
    {
        public SearchQuery? query { get; set; }
    }

    public class SearchQuery
    {
        public List<SearchItem>? search { get; set; }
    }

    public class SearchItem
    {
        public long pageid { get; set; }
        public string? title { get; set; }
    }

    public class PageImagesResponse
    {
        public PageImagesQuery? query { get; set; }
    }

    public class PageImagesQuery
    {
        public Dictionary<string, WikiPage>? pages { get; set; }
    }

    public class WikiPage
    {
        public WikiThumb? thumbnail { get; set; }
    }

    public class WikiThumb
    {
        public string? source { get; set; }
    }
}
