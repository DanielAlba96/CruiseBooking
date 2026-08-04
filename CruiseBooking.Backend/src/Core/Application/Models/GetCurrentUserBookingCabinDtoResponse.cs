using System.Collections.Generic;

namespace Core.Application.Models;

/// <summary>
/// DTO que representa la información de una cabina asociada a una reserva del usuario actual.
/// </summary>
public class GetCurrentUserBookingCabinDtoResponse
{
    /// <value>Identificador único de la cabina.</value>
    public int Id { get; set; }

    /// <value>Nombre o tipo de la cabina.</value>
    public string TypeName { get; set; } = string.Empty;

    /// <value>Descripción detallada del tipo de cabina.</value>
    public string TypeDescription { get; set; } = string.Empty;

    /// <value>Cantidad de ocupantes permitidos en la cabina.</value>
    public int Occupants { get; set; }

    /// <value>Precio de la cabina.</value>
    public decimal Price { get; set; }

    /// <value>Lista de pasajeros alojados en la cabina.</value>
    public List<GetCurrentUserBookingPassengerDtoResponse> Passengers { get; set; } = new();
}
