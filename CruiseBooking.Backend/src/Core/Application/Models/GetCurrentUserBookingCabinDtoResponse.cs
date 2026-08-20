using System.Collections.Generic;

namespace Core.Application.Models;

/// <summary>
/// DTO que representa la información de un camarote asociado a una reserva del usuario actual.
/// </summary>
public class GetCurrentUserBookingCabinDtoResponse
{
    /// <value>Identificador único del camarote.</value>
    public int Id { get; set; }

    /// <value>Nombre o tipo del camarote.</value>
    public string TypeName { get; set; } = string.Empty;

    /// <value>Descripción detallada del tipo de camarote.</value>
    public string TypeDescription { get; set; } = string.Empty;

    /// <value>Cantidad de ocupantes permitidos en el camarote.</value>
    public int Occupants { get; set; }

    /// <value>Precio del camarote.</value>
    public decimal Price { get; set; }

    /// <value>Lista de pasajeros alojados en el camarote.</value>
    public List<GetCurrentUserBookingPassengerDtoResponse> Passengers { get; set; } = new();
}
