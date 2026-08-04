namespace Core.Application.Models;

/// <summary>Camarote bloqueado que el cliente solicita incluir en la reserva.</summary>
/// <param name="CabinId">Identificador de la cabina del barco.</param>
/// <param name="Occupants">Número de pasajeros que ocuparán la cabina, tal y como se bloqueó.</param>
/// <param name="Price">Precio de la unidad de cabina, tal y como se bloqueó.</param>
public sealed record BookingCabinSelection(int CabinId, int Occupants, decimal Price);
