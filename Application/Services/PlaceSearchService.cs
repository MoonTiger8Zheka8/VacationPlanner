using VacationPlanner.Application.Interfaces;
using VacationPlanner.Domain.Enums;
using VacationPlanner.Domain.Models;
using VacationPlanner.Infrastructure.ApiClients;

namespace VacationPlanner.Application.Services;

public class PlaceSearchService : IPlaceSearchService
{
    private readonly GeoCodingClient _geo;
    private readonly OverpassClient _overpass;

    public PlaceSearchService(GeoCodingClient geo, OverpassClient overpass)
    {
        _geo = geo;
        _overpass = overpass;
    }

    public async Task<List<PlaceInfo>> SearchAsync(string destination, VacationType type, int limit = 12)
    {
        var coord = await _geo.GetCoordinatesAsync($"{destination}, Ukraine");
        if (coord == null)
        {
            Console.WriteLine($"[Geo] Not found coordinates for: {destination}, Ukraine");
            return new List<PlaceInfo>();
        }

        var filter = GetOverpassFilter(type);

        // 15 км радіус
        var places = await _overpass.SearchPlacesAsync(
            coord.Value.lat,
            coord.Value.lon,
            radiusMeters: 15000,
            overpassFilter: filter,
            limit: limit);

        Console.WriteLine($"[OSM] destination={destination}, type={type}, found={places.Count}");

        return places
            .GroupBy(p => p.Name)
            .Select(g => g.First())
            .ToList();
    }

    private static string GetOverpassFilter(VacationType type)
    {
        return type switch
        {
            VacationType.Excursions =>
                @"[tourism~""attraction|museum|gallery""]",
            VacationType.Relax =>
                @"[leisure~""park|garden""]",
            VacationType.Active =>
                @"[natural~""peak|waterfall|wood""]",
            VacationType.Beach =>
                @"[natural~""beach""]",
            _ =>
                @"[tourism~""attraction""]"
        };
    }
}
