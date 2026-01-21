namespace VacationPlanner.Domain.Models;

public class VacationPlan
{
    public VacationRequest Request { get; set; } = new();
    public List<DayPlan> Days { get; set; } = new();

    public decimal TotalEstimatedCost => Days.Sum(d => d.EstimatedCost);
}
