namespace Core.Agents.Models;

/// <summary>
/// Borrador de reserva en construcción durante una conversación de chat.
/// Los precios son instantáneas tomadas del servidor en el momento de añadir cada elemento.
/// </summary>
internal sealed class BookingDraft
{
    /// <summary>Identificador de la fecha de crucero.</summary>
    public int CruiseDateId { get; set; }

    /// <summary>Nombre del crucero.</summary>

    public string CruiseName { get; set; } = string.Empty;

    /// <summary>Identificador del barco.</summary>
    public int ShipId { get; set; }

    /// <summary>Nombre del barco.</summary>
    public string ShipName { get; set; } = string.Empty;

    /// <summary>Fecha de salida.</summary>
    public DateTime DepartureDate { get; set; } = DateTime.MinValue;

    /// <summary>Lista de camarotes seleccionados en el borrador.</summary>
    public List<DraftCabin> Cabins { get; set; } = [];

    /// <summary>Lista de extras seleccionados en el borrador.</summary>
    public List<DraftExtra> Extras { get; set; } = [];
}

/// <summary>
/// Camarote seleccionado dentro de un <see cref="BookingDraft"/>, con su precio y aforo máximo.
/// Los datos de los pasajeros se recogen después de la reserva mediante el check-in online.
/// </summary>
internal sealed class DraftCabin
{
    /// <summary>Identificador único del camarote.</summary>
    public int Id { get; set; }

    /// <summary>Nombra del camarote.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Precio del camarote en el momento de añadirse al borrador.</summary>
    public decimal Price { get; set; }

    /// <summary>Cantidad de pasajeros que ocuparán el camarote.</summary>
    public int Occupants { get; set; }
}

/// <summary>
/// Extra seleccionado dentro de un <see cref="BookingDraft"/>, con su nombre y precio.
/// </summary>
internal sealed class DraftExtra
{
    /// <summary>Identificador único del extra.</summary>
    public int ExtraId { get; set; }

    /// <summary>Nombre descriptivo del extra.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Precio del extra en el momento de añadirse al borrador.</summary>
    public decimal Price { get; set; }
}
