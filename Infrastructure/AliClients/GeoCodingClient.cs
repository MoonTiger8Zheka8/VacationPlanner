using System.Net.Http.Json;

namespace VacationPlanner.Infrastructure.ApiClients;

public class GeoCodingClient
{
    private readonly HttpClient _http;

    public GeoCodingClient(HttpClient http)
    {
        _http = http;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("VacationPlannerApp/1.0"); // потрібно Nominatim
    }

    public async Task<(double lat, double lon)?> GetCoordinatesAsync(string query)
    {
        var url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(query)}";

        var res = await _http.GetFromJsonAsync<List<NominatimResult>>(url);
        var first = res?.FirstOrDefault();

        if (first == null) return null;

        if (double.TryParse(first.lat, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var lat) &&
            double.TryParse(first.lon, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var lon))
        {
            return (lat, lon);
        }

        return null;
    }

    public class NominatimResult
    {
        public string lat { get; set; } = "";
        public string lon { get; set; } = "";
    }
}
