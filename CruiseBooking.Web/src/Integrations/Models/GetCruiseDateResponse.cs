namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa la información detallada de una fecha específica de crucero.
/// </summary>
/// <param name="Name">El nombre del crucero.</param>
/// <param name="OriginPort">El puerto desde el que sale el crucero.</param>
/// <param name="StartDate">La fecha en que comienza el crucero.</param>
/// <param name="DurationInDays">La duración del crucero en días.</param>
/// <param name="PaymentStartDate">La fecha en que el pago está disponible.</param>
/// <param name="CheckInStartDate">La fecha en que el check-in está disponible.</param>
/// <param name="AvailableCabins">La lista de camarotes disponibles para esta fecha de crucero.</param>
/// <param name="AvailableExtras">La lista de extras disponibles para esta fecha de crucero.</param>
public record GetCruiseDateResponse(
    string Name,
    string OriginPort,
    DateTime StartDate,
    int DurationInDays,
    DateTime PaymentStartDate,
    DateTime CheckInStartDate,
    List<CruiseDateCabinResponse> AvailableCabins,
    List<CruiseDateExtraResponse> AvailableExtras
);

/// <summary>
/// Representa un camarote disponible en una fecha específica de crucero.
/// </summary>
/// <param name="Id">El identificador único del camarote.</param>
/// <param name="Name">El nombre o tipo del camarote.</param>
/// <param name="Description">Una descripción de las características del camarote.</param>
/// <param name="MaxOccupancy">El número máximo de ocupantes permitidos en el camarote.</param>
/// <param name="AvailableCount">El número de camarotes disponibles de este tipo.</param>
/// <param name="FullPrice">El precio completo del camarote.</param>
public record CruiseDateCabinResponse(
    int Id,
    string Name,
    string Description,
    int MaxOccupancy,
    int AvailableCount,
    decimal FullPrice
);

/// <summary>
/// Representa un servicio adicional disponible en una fecha específica de crucero.
/// </summary>
/// <param name="Id">El identificador único del extra.</param>
/// <param name="Name">El nombre del servicio adicional.</param>
/// <param name="Description">Una descripción del servicio adicional.</param>
/// <param name="Price">El precio del servicio adicional.</param>
public record CruiseDateExtraResponse(
    int Id,
    string Name,
    string Description,
    decimal Price
);
