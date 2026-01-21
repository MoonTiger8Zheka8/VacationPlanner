using VacationPlanner.Domain.Models;

namespace VacationPlanner.Application.Interfaces;

public interface IVoiceCommandParser
{
    VacationRequest Parse(string inputText);
}
