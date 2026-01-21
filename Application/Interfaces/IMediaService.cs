using VacationPlanner.Domain.Models;

namespace VacationPlanner.Application.Interfaces;

public interface IMediaService
{
    Task<PlaceInfo> EnrichPlaceAsync(PlaceInfo place, string destination);
}
