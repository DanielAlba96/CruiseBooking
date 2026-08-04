namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa una solicitud para bloquear un camarote para una reserva.
/// </summary>
/// <param name="CruiseDateId">El identificador único de la fecha del crucero.</param>
/// <param name="Price">El precio del camarote en el momento del bloqueo.</param>
/// <param name="Occupants">El número de ocupantes para el camarote.</param>
public record LockCabinRequest(int CruiseDateId, decimal Price, int Occupants);

/// <summary>
/// Representa la respuesta después de bloquear un camarote.
/// </summary>
/// <param name="CabinId">El identificador único del camarote bloqueado.</param>
/// <param name="Occupants">El número de ocupantes para el camarote.</param>
/// <param name="Price">El precio bloqueado del camarote.</param>
public record LockedCabinResponse(int CabinId, int Occupants, decimal Price);
