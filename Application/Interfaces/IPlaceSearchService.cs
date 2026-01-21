using VacationPlanner.Domain.Enums;
using VacationPlanner.Domain.Models;

namespace VacationPlanner.Application.Interfaces;

public interface IPlaceSearchService
{
    Task<List<PlaceInfo>> SearchAsync(string destination, VacationType type, int limit = 12);
}
