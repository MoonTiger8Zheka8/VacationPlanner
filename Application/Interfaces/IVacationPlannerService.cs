using VacationPlanner.Domain.Models;

namespace VacationPlanner.Application.Interfaces;

public interface IVacationPlannerService
{
    Task<VacationPlan> GeneratePlanAsync(VacationRequest request);
}
