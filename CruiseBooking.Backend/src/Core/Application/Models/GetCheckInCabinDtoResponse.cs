namespace Core.Application.Models;

/// <summary>Camarote de la reserva pendiente de check-in, con su aforo máximo y los pasajeros ya registrados.</summary>
public class GetCheckInCabinDtoResponse
{
    /// <summary>Identificador único del camarote en la reserva.</summary>
    /// <value>ID del camarote asignado a la reserva.</value>
    public int BookingCabinId { get; set; }

    /// <summary>Tipo de camarote.</summary>
    /// <value>Clasificación del camarote (ej: Interior, Exterior, Suite).</value>
    public string CabinType { get; set; } = string.Empty;

    /// <summary>Descripción del camarote.</summary>
    /// <value>Detalles y características del camarote.</value>
    public string CabinDescription { get; set; } = string.Empty;

    /// <summary>Aforo máximo del camarote.</summary>
    /// <value>Número máximo de pasajeros que puede albergar.</value>
    public int Occupancy { get; set; }

    /// <summary>Lista de pasajeros registrados en el check-in.</summary>
    /// <value>Colección de pasajeros ya registrados en este camarote.</value>
    public List<GetCheckInPassengerDtoResponse> Passengers { get; set; } = [];
}
