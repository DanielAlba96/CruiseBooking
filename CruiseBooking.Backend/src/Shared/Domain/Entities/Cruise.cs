namespace Shared.Domain.Entities;

public class Cruise
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Zone { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string OriginPort { get; set; } = null!;
    public string Itinerary { get; set; } = null!;
    public int DurationInDays { get; set; }
    public bool AdultsOnly { get; set; }
    public bool Featured { get; set; }

    public ICollection<CruiseDate> CruiseDates { get; set; } = [];
}
