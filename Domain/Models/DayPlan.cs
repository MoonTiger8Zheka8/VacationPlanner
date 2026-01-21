namespace VacationPlanner.Domain.Models;

public class DayPlan
{
    public int DayNumber { get; set; }
    public string Title { get; set; } = "";
    public List<PlaceInfo> Places { get; set; } = new();
    public decimal EstimatedCost { get; set; }
}
