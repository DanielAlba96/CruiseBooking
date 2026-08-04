namespace CruiseBooking.GraphQL.Models;

public class CruiseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OriginPort { get; set; } = string.Empty;
    public string Itinerary { get; set; } = string.Empty;
    public int DurationInDays { get; set; }
    public List<CruiseDateDto> Dates { get; set; } = [];
}

public class CruiseDateDto
{
    public int Id { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public ShipDto? Ship { get; set; }
    public List<ExtraDto> Extras { get; set; } = [];
}

public class ShipDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<CabinDto> Cabins { get; set; } = [];
}

public class CabinDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Number { get; set; }
    public decimal Price { get; set; }
    public int MaxOccupancy { get; set; }
}

public class ExtraDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}