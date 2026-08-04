namespace Core.Infrastructure.AI.Models;

/// <summary>
/// Borrador de reserva en construcción durante una conversación de chat.
/// Los precios son instantáneas tomadas del servidor en el momento de añadir cada elemento.
/// </summary>
internal sealed class BookingDraft
{
    /// <summary>Identificador de la fecha de crucero.</summary>
    public int CruiseDateId { get; set; }

    /// <summary>Identificador del barco.</summary>
    public int ShipId { get; set; }

    /// <summary>Lista de cabinas seleccionadas en el borrador.</summary>
    public List<DraftCabin> Cabins { get; } = [];

    /// <summary>Lista de extras seleccionados en el borrador.</summary>
    public List<DraftExtra> Extras { get; } = [];
}

/// <summary>
/// Cabina seleccionada dentro de un <see cref="BookingDraft"/>, con su precio y aforo máximo.
/// Los datos de los pasajeros se recogen después de la reserva mediante el check-in online.
/// </summary>
internal sealed class DraftCabin
{
    /// <summary>Identificador único de la cabina.</summary>
    public int CabinId { get; set; }

    /// <summary>Precio de la cabina en el momento de añadirse al borrador.</summary>
    public decimal Price { get; set; }

    /// <summary>Aforo máximo de pasajeros en la cabina.</summary>
    public int MaxOccupancy { get; set; }

    /// <summary>Cantidad de pasajeros que ocuparán la cabina.</summary>
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
