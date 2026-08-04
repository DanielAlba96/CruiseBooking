using CruiseBooking.Data.GraphQL;
using CruiseBooking.GraphQL.Models;

namespace CruiseBooking.Mappings;

/// <summary>
/// Proporciona métodos de extensión para asignar modelos de GraphQL a DTOs de dominio.
/// </summary>
public static class MapperConfiguration
{
    /// <summary>
    /// Convierte un resultado de búsqueda de crucero de GraphQL a un DTO de dominio.
    /// </summary>
    /// <param name="graphqlCruise">El resultado de búsqueda de crucero de GraphQL.</param>
    /// <returns>Un DTO de crucero de dominio con información de fechas, barco y extras.</returns>
    public static CruiseDto ToCruiseDto(this ISearchCruises_Cruises_Nodes graphqlCruise) => new()
    {
        Id = graphqlCruise.Id,
        Name = graphqlCruise.Name,
        Code = graphqlCruise.Code,
        Zone = graphqlCruise.Zone,
        Description = graphqlCruise.Description,
        OriginPort = graphqlCruise.OriginPort,
        Itinerary = graphqlCruise.Itinerary,
        DurationInDays = graphqlCruise.DurationInDays,
        Dates = graphqlCruise.Dates.Select(date => new CruiseDateDto
        {
            Id = date.Id,
            StartDate = date.StartDate,
            Ship = date.Ship is null ? null : new ShipDto
            {
                Name = date.Ship.Name,
                Cabins = date.Ship.Cabins.Select(cabin => new CabinDto
                {
                    Name = cabin.Name ?? string.Empty,
                    Price = cabin.Price,
                }).ToList(),
            },
            Extras = date.Extras.Select(extra => new ExtraDto
            {
                Name = extra.Name,
                Price = extra.Price,
            }).ToList(),
        }).ToList(),
    };

    /// <summary>
    /// Convierte un resultado de crucero destacado de GraphQL a un DTO de dominio.
    /// </summary>
    /// <param name="graphqlCruise">El resultado de crucero destacado de GraphQL.</param>
    /// <returns>Un DTO de crucero de dominio con información básica.</returns>
    public static CruiseDto ToCruiseDto(this IGetFeaturedCruises_FeaturedCruises graphqlCruise) => new()
    {
        Id = graphqlCruise.Id,
        Name = graphqlCruise.Name,
        Code = graphqlCruise.Code,
    };

    /// <summary>
    /// Convierte un resultado de barco de GraphQL a un DTO de dominio.
    /// </summary>
    /// <param name="graphqlShip">El resultado de barco de GraphQL.</param>
    /// <returns>Un DTO de barco de dominio.</returns>
    public static ShipDto ToDto(this IGetShips_Ships graphqlShip) => new()
    {
        Code = graphqlShip.Code,
        Name = graphqlShip.Name,
        Description = graphqlShip.Description,
    };
}