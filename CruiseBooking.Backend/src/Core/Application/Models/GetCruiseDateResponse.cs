namespace Core.Application.Models;

/// <summary>Response payload for a single cruise date, including cruise info, available cabins, and available extras.</summary>
public record GetCruiseDateResponse(
    string Name,
    string OriginPort,
    DateTime StartDate,
    int DurationInDays,
    List<CruiseDateCabinResponse> AvailableCabins,
    List<CruiseDateExtraResponse> AvailableExtras
);

/// <summary>Cabin type offered on a cruise date with its current availability count and adjusted price.</summary>
public record CruiseDateCabinResponse(
    int Id,
    string Name,
    string Description,
    int MaxOccupancy,
    int AvailableCount,
    decimal FullPrice
);

/// <summary>Extra offered on a cruise date with its adjusted price.</summary>
public record CruiseDateExtraResponse(
    int Id,
    string Name,
    string Description,
    decimal Price
);
