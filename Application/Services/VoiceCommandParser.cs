using System.Text.RegularExpressions;
using VacationPlanner.Application.Interfaces;
using VacationPlanner.Domain.Enums;
using VacationPlanner.Domain.Models;

namespace VacationPlanner.Application.Services;

public class VoiceCommandParser : IVoiceCommandParser
{
    public VacationRequest Parse(string inputText)
    {
        var text = Normalize(inputText);

        var request = new VacationRequest
        {
            Destination = ExtractDestination(text) ?? "Львів",
            Days = ExtractDays(text) ?? 3,
            Budget = ExtractBudget(text) ?? 10000,
            People = ExtractPeople(text) ?? 1,
            Type = ExtractType(text)
        };

        return request;
    }

    private static string Normalize(string input)
        => input.Trim().ToLowerInvariant();

    private static int? ExtractDays(string text)
    {
        // "на 5 днів", "5 днів", "на 7 дні"
        var match = Regex.Match(text, @"(\d{1,2})\s*(дн(і|ів|я))");
        if (match.Success && int.TryParse(match.Groups[1].Value, out int days))
            return days;

        return null;
    }

    private static decimal? ExtractBudget(string text)
    {
        // "бюджет 15000", "15000 грн", "15 000 гривень"
        var match = Regex.Match(text, @"(\d{1,3}(\s?\d{3})*)\s*(грн|гривень|₴)?");
        if (match.Success)
        {
            var numberText = match.Groups[1].Value.Replace(" ", "");
            if (decimal.TryParse(numberText, out decimal budget))
                return budget;
        }
        return null;
    }

    private static int? ExtractPeople(string text)
    {
        // "для 2 людей", "2 особи"
        var match = Regex.Match(text, @"(для\s*)?(\d{1,2})\s*(люд(ей|ини)|осіб(и)?)");
        if (match.Success && int.TryParse(match.Groups[2].Value, out int people))
            return people;

        return null;
    }

    private static string? ExtractDestination(string text)
    {
        // "у львів", "в київ", "до одеси"
        var match = Regex.Match(text, @"(у|в|до)\s+([а-яіїєґA-Za-z\-]+)");
        if (match.Success)
            return Capitalize(match.Groups[2].Value);

        return null;
    }

    private static VacationType ExtractType(string text)
    {
        if (text.Contains("пляж") || text.Contains("море"))
            return VacationType.Beach;

        if (text.Contains("актив") || text.Contains("похід"))
            return VacationType.Active;

        if (text.Contains("релакс") || text.Contains("спокі"))
            return VacationType.Relax;

        // за замовчуванням
        return VacationType.Excursions;
    }

    private static string Capitalize(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return word;
        return char.ToUpper(word[0]) + word[1..];
    }
}
