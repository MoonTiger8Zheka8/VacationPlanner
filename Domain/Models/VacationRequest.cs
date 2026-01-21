using VacationPlanner.Domain.Enums;

namespace VacationPlanner.Domain.Models;

public class VacationRequest
{
    public string Destination { get; set; } = "";
    public int Days { get; set; } = 3;
    public decimal Budget { get; set; } = 10000;
    public int People { get; set; } = 1;

    public VacationType Type { get; set; } = VacationType.Excursions;
}
