namespace Core.Application.Models;

/// <summary>Camarote bloqueado que el cliente solicita incluir en la reserva.</summary>
/// <param name="CabinId">Identificador del camarote del barco.</param>
/// <param name="Occupants">Número de pasajeros que ocuparán el camarote, tal y como se bloqueó.</param>
/// <param name="Price">Precio de la unidad de camarote, tal y como se bloqueó.</param>
public sealed record BookingCabinSelection(int CabinId, int Occupants, decimal Price);
