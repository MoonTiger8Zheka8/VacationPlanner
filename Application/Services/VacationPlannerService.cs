using VacationPlanner.Application.Interfaces;
using VacationPlanner.Domain.Enums;
using VacationPlanner.Domain.Models;

namespace VacationPlanner.Application.Services;

public class VacationPlannerService : IVacationPlannerService
{
    private readonly IMediaService _media;
    private readonly IPlaceSearchService _search;

    public VacationPlannerService(IMediaService media, IPlaceSearchService search)
    {
        _media = media;
        _search = search;
    }
    const int MaxPlacesPerDay = 6;
    VacationPlan tempPlan = new VacationPlan();
    public async Task<VacationPlan> GeneratePlanAsync(VacationRequest request)
    {
        var plan = new VacationPlan { Request = request };

        // реальні місця
        var places = await _search.SearchAsync(request.Destination, request.Type, limit: 12);

        // fallback
        if (places.Count == 0)
        {
            places = GetPlacesFallback(request.Destination, request.Type);
            return tempPlan;
        }

        //фото/опис
        foreach (var p in places)
            await _media.EnrichPlaceAsync(p, request.Destination);
        
        //по днях
        var placesPerDay = Math.Max(2, (int)Math.Ceiling(places.Count / (double)request.Days));
        
        placesPerDay = Math.Min(placesPerDay, MaxPlacesPerDay);

        int index = 0;
        for (int day = 1; day <= request.Days; day++)
        {
            var dayPlaces = places
                .Skip(index)
                .Take(placesPerDay)
                .ToList();

            index += placesPerDay;

            if (dayPlaces.Count == 0)
                break;

            plan.Days.Add(new DayPlan
            {
                DayNumber = day,
                Title = GetDayTitle(request.Type, day),
                Places = dayPlaces,
                EstimatedCost = EstimateCost(request.Type, request.People)
            });
        }
        tempPlan = plan;
        return plan;
    }

    private static string GetDayTitle(VacationType type, int day)
    {
        return type switch
        {
            VacationType.Relax => $"День {day}: Відпочинок і прогулянки",
            VacationType.Active => $"День {day}: Активності та природа",
            VacationType.Excursions => $"День {day}: Пам’ятки та культура",
            VacationType.Beach => $"День {day}: Море та пляж",
            _ => $"День {day}"
        };
    }

    private static decimal EstimateCost(VacationType type, int people)
    {
        var baseCost = type switch
        {
            VacationType.Relax => 800,
            VacationType.Active => 1200,
            VacationType.Excursions => 1000,
            VacationType.Beach => 1500,
            _ => 1000
        };

        return baseCost * Math.Max(1, people);
    }

    private static List<PlaceInfo> GetPlacesFallback(string destination, VacationType type)
    {
        // пошук недоступний
        return new List<PlaceInfo>
        {
            new PlaceInfo { Name = $"{destination} main square", Description = "Центральна локація." },
            new PlaceInfo { Name = $"{destination} museum", Description = "Культурна локація." },
            new PlaceInfo { Name = $"{destination} park", Description = "Прогулянка та відпочинок." }
        };
    }
}
