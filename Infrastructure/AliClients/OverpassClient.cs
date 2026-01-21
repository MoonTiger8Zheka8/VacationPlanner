using System.Globalization;
using System.Net.Http.Json;
using VacationPlanner.Domain.Models;

namespace VacationPlanner.Infrastructure.ApiClients;

public class OverpassClient
{
    private readonly HttpClient _http;

    public OverpassClient(HttpClient http)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://overpass-api.de/api/");
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("VacationPlannerApp/1.0");
    }

    public async Task<List<PlaceInfo>> SearchPlacesAsync(
        double lat,
        double lon,
        int radiusMeters,
        string overpassFilter,
        int limit = 15)
    {
        // overpass QL query

        var query = $@"
            [out:json][timeout:25];
                (
                node(around:{radiusMeters},{lat.ToString(CultureInfo.InvariantCulture)},{lon.ToString(CultureInfo.InvariantCulture)}){overpassFilter};
                way(around:{radiusMeters},{lat.ToString(CultureInfo.InvariantCulture)},{lon.ToString(CultureInfo.InvariantCulture)}){overpassFilter};
                relation(around:{radiusMeters},{lat.ToString(CultureInfo.InvariantCulture)},{lon.ToString(CultureInfo.InvariantCulture)}){overpassFilter};
                );
            out center tags;
        ";

        var body = new Dictionary<string, string>
        {
            ["data"] = query
        };

        HttpResponseMessage resp;
        try
        {
            resp = await _http.PostAsync("interpreter", new FormUrlEncodedContent(body));
        }
        catch
        {
            return new List<PlaceInfo>();
        }

        if (!resp.IsSuccessStatusCode)
            return new List<PlaceInfo>();

        OverpassResponse? json;
        try
        {
            json = await resp.Content.ReadFromJsonAsync<OverpassResponse>();
        }
        catch
        {
            return new List<PlaceInfo>();
        }

        var results = new List<PlaceInfo>();

        foreach (var el in json?.elements ?? new List<Element>())
        {
            var name = el.tags?.TryGetValue("name", out var n) == true ? n : null;
            if (string.IsNullOrWhiteSpace(name)) continue;

            double? plat = el.lat;
            double? plon = el.lon;

            if (plat == null || plon == null)
            {
                plat = el.center?.lat;
                plon = el.center?.lon;
            }

            if (plat == null || plon == null) continue;

            results.Add(new PlaceInfo
            {
                Id = $"{el.type}_{el.id}",//
                Name = name,
                Description = "",
                Lat = plat,
                Lon = plon
            });

            if (results.Count >= limit) break;
        }

        return results;//
    }

    // DTO
    public class OverpassResponse
    {
        public List<Element> elements { get; set; } = new();
    }

    public class Element
    {
        public string type { get; set; } = "";
        public long id { get; set; }
        public double? lat { get; set; }
        public double? lon { get; set; }
        public Center? center { get; set; }
        public Dictionary<string, string>? tags { get; set; }
    }

    public class Center
    {
        public double? lat { get; set; }
        public double? lon { get; set; }
    }
}
