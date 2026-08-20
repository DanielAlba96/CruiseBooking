namespace Core.Application.Models;

/// <summary>Datos de la reserva que necesita el formulario de check-in online.</summary>
public class GetCheckInDtoResponse
{
    /// <summary>Identificador único de la reserva.</summary>
    /// <value>Número entero que identifica la reserva.</value>
    public int BookingId { get; set; }

    /// <summary>Nombre del crucero.</summary>
    /// <value>Nombre de la embarcación.</value>
    public string CruiseName { get; set; } = string.Empty;

    /// <summary>Descripción del crucero.</summary>
    /// <value>Información detallada sobre el crucero.</value>
    public string CruiseDescription { get; set; } = string.Empty;

    /// <summary>Puerto de origen del crucero.</summary>
    /// <value>Nombre del puerto de salida.</value>
    public string Origin { get; set; } = string.Empty;

    /// <summary>Fecha y hora de inicio del crucero.</summary>
    /// <value>Fecha de embarque (UTC).</value>
    public DateTime StartDate { get; set; }

    /// <summary>Fecha y hora de fin del crucero.</summary>
    /// <value>Fecha de desembarque (UTC).</value>
    public DateTime EndDate { get; set; }

    /// <summary>Lista de camarotes asociados a la reserva.</summary>
    /// <value>Colección de detalles de camarotes.</value>
    public List<GetCheckInCabinDtoResponse> Cabins { get; set; } = [];
}
