using System.Globalization;
using System.Net.Http.Json;
using VacationPlanner.Domain.Models;

namespace VacationPlanner.Infrastructure.ApiClients;

public class OpenTripMapClient
{
    private readonly HttpClient _http;
    private readonly OpenTripMapOptions _options;

    public OpenTripMapClient(HttpClient http, OpenTripMapOptions options)
    {
        _http = http;
        _options = options;

        _http.BaseAddress = new Uri("https://api.opentripmap.com/0.1/en/");
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("VacationPlannerApp/1.0");
    }

    /// <summary>
    /// Пошук місць через bbox (найстабільніший метод).
    /// deltaLat/deltaLon — “розмір” квадрату навколо міста.
    /// </summary>
    public async Task<List<PlaceInfo>> GetPlacesByBBoxAsync(
        double lat,
        double lon,
        double deltaLat,
        double deltaLon,
        string kinds,
        int limit = 12)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            Console.WriteLine("[OTM] ApiKey is EMPTY!");
            return new List<PlaceInfo>();
        }

        kinds = (kinds ?? "").Replace(" ", "");

        var latMin = (lat - deltaLat).ToString(CultureInfo.InvariantCulture);
        var latMax = (lat + deltaLat).ToString(CultureInfo.InvariantCulture);
        var lonMin = (lon - deltaLon).ToString(CultureInfo.InvariantCulture);
        var lonMax = (lon + deltaLon).ToString(CultureInfo.InvariantCulture);

        // kinds опціональні
        var kindsPart = string.IsNullOrWhiteSpace(kinds) ? "" : $"&kinds={kinds}";

        var url =
            $"places/bbox?lon_min={lonMin}&lat_min={latMin}&lon_max={lonMax}&lat_max={latMax}" +
            $"{kindsPart}" +
            $"&format=json" +
            $"&limit={limit}" +
            $"&apikey={_options.ApiKey}";

        Console.WriteLine("[OTM] bbox URL: " + _http.BaseAddress + url);

        HttpResponseMessage resp;
        try
        {
            resp = await _http.GetAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[OTM] bbox request error: " + ex.Message);
            return new List<PlaceInfo>();
        }

        Console.WriteLine("[OTM] bbox status=" + (int)resp.StatusCode);

        var body = await resp.Content.ReadAsStringAsync();
        Console.WriteLine("[OTM] bbox body=" + body[..Math.Min(body.Length, 200)]);

        if (!resp.IsSuccessStatusCode)
            return new List<PlaceInfo>();

        BBoxResponse? raw;
        try
        {
            raw = await resp.Content.ReadFromJsonAsync<BBoxResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[OTM] bbox json error: " + ex.Message);
            return new List<PlaceInfo>();
        }

        var items = raw?.features ?? new List<BBoxFeature>();
        Console.WriteLine("[OTM] bbox features=" + items.Count);

        var results = new List<PlaceInfo>();

        // bbox дає список xid → потім по xid беремо деталі
        foreach (var f in items)
        {
            var xid = f?.properties?.xid;
            if (string.IsNullOrWhiteSpace(xid)) continue;

            var details = await GetPlaceDetailsAsync(xid);
            if (details == null) continue;
            if (string.IsNullOrWhiteSpace(details.name)) continue;

            results.Add(new PlaceInfo
            {
                Id = details.xid ?? Guid.NewGuid().ToString("N"),
                Name = details.name,
                Description = details.wikipedia_extracts?.text ?? details.info?.descr ?? "",
                Address = details.address?.road ?? details.address?.city,
                Lat = details.point?.lat,
                Lon = details.point?.lon,
                ImageUrl = "",
                WikipediaUrl = details.wikipedia
            });

            if (results.Count >= limit)
                break;
        }

        return results;
    }

    private async Task<PlaceDetails?> GetPlaceDetailsAsync(string xid)
    {
        var url = $"places/xid/{Uri.EscapeDataString(xid)}?apikey={_options.ApiKey}";
        try
        {
            return await _http.GetFromJsonAsync<PlaceDetails>(url);
        }
        catch
        {
            return null;
        }
    }

    // ---------- DTO (bbox) ----------
    public class BBoxResponse
    {
        public List<BBoxFeature> features { get; set; } = new();
    }

    public class BBoxFeature
    {
        public BBoxProperties? properties { get; set; }
    }

    public class BBoxProperties
    {
        public string? xid { get; set; }
        public string? name { get; set; }
        public string? kinds { get; set; }
    }

    // ---------- DTO (details xid) ----------
    public class PlaceDetails
    {
        public string? xid { get; set; }
        public string? name { get; set; }
        public Point? point { get; set; }
        public Address? address { get; set; }

        public WikiExtracts? wikipedia_extracts { get; set; }
        public PlaceInfoDetails? info { get; set; }

        public string? wikipedia { get; set; } // url
        public string? kinds { get; set; }
    }

    public class Point
    {
        public double? lon { get; set; }
        public double? lat { get; set; }
    }

    public class Address
    {
        public string? road { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? country { get; set; }
    }

    public class WikiExtracts
    {
        public string? text { get; set; }
    }

    public class PlaceInfoDetails
    {
        public string? descr { get; set; }
    }
}
