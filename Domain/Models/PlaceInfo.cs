namespace VacationPlanner.Domain.Models;

public class PlaceInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string? Address { get; set; }

    // Опційно для мапи
    public double? Lat { get; set; }
    public double? Lon { get; set; }
    public string? WikipediaUrl { get; set; }
}
