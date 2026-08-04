namespace CruiseBooking.Vms;

/// <summary>
/// Representa un camarote seleccionado por el usuario durante la reserva.
/// </summary>
public class CabinSelectionVm
{
    /// <summary>
    /// Obtiene o establece el identificador único del camarote.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Obtiene o establece el nombre del tipo de camarote.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la descripción del camarote.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el número del camarote.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Obtiene o establece la ocupancia máxima del camarote.
    /// </summary>
    public int MaxOccupancy { get; set; }

    /// <summary>
    /// Obtiene o establece el número actual de ocupantes en el camarote.
    /// </summary>
    public int CurrentOccupancy { get; set; }

    /// <summary>
    /// Obtiene o establece la lista de ocupantes asignados a este camarote.
    /// </summary>
    public List<OccupantVm> Occupants { get; set; } = [];

    /// <summary>
    /// Obtiene o establece el precio completo del camarote.
    /// </summary>
    public decimal FullPrice { get; set; }

    /// <summary>
    /// Obtiene el precio actual del camarote calculado en función del número de ocupantes.
    /// </summary>
    public decimal CurrentPrice => Math.Round(FullPrice / MaxOccupancy, 2) * CurrentOccupancy;
}
