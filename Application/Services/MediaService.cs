using VacationPlanner.Application.Interfaces;
using VacationPlanner.Domain.Models;
using VacationPlanner.Infrastructure.ApiClients;

namespace VacationPlanner.Application.Services;

public class MediaService : IMediaService
{
    private readonly WikipediaClient _wiki;
    private readonly GeoCodingClient _geo;

    public MediaService(WikipediaClient wiki, GeoCodingClient geo)
    {
        _wiki = wiki;
        _geo = geo;
    }

    public async Task<PlaceInfo> EnrichPlaceAsync(PlaceInfo place, string destination)
    {
        //Wikipedia фото
        if (string.IsNullOrWhiteSpace(place.ImageUrl))
        {
            string? img = null;

            // title
            if (!string.IsNullOrWhiteSpace(place.WikipediaUrl))
            {
                var title = ExtractWikiTitle(place.WikipediaUrl);
                if (!string.IsNullOrWhiteSpace(title))
                {
                    img = await _wiki.TryGetImageAsync(title);
                }
            }

            // по назві місця
            img ??= await _wiki.TryGetImageAsync(place.Name);

            // По назві + місто
            img ??= await _wiki.TryGetImageAsync($"{place.Name} {destination}");
            img ??= await _wiki.TryGetImageAsync(destination);
            img ??= await _wiki.TryGetImageAsync($"{place.Name} Україна");

            if (!string.IsNullOrWhiteSpace(img))
                place.ImageUrl = img;
        }

        // Якщо координат нема
        if (place.Lat is null || place.Lon is null)
        {
            var coord = await _geo.GetCoordinatesAsync($"{place.Name}, {destination}, Ukraine");
            if (coord != null)
            {
                place.Lat = coord.Value.lat;
                place.Lon = coord.Value.lon;
            }
        }

        //fallback image
        if (string.IsNullOrWhiteSpace(place.ImageUrl))
            place.ImageUrl = "/images/placeholders/place.jpg";
        Console.WriteLine($"[WIKI] place={place.Name}, img={(string.IsNullOrWhiteSpace(place.ImageUrl) ? "NO" : "YES")}");
        return place;
    }

    private static string? ExtractWikiTitle(string wikipediaUrl)
    {
        // приклад: https://en.wikipedia.org/wiki/Lviv_Opera
        var parts = wikipediaUrl.Split("/wiki/");
        if (parts.Length < 2) return null;

        var title = parts[^1];
        title = Uri.UnescapeDataString(title.Replace("_", " "));

        // якір #
        var hash = title.IndexOf('#');
        if (hash >= 0) title = title[..hash];

        return title;
    }
}
