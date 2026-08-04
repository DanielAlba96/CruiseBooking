namespace CruiseBooking.Vms;

/// <summary>
/// Representa un crucero seleccionado por el usuario durante la reserva.
/// </summary>
public class CruiseSelectionVm
{
    /// <summary>
    /// Obtiene o establece el identificador único del crucero.
    /// </summary>
    public int CruiseId { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador único de la fecha del crucero.
    /// </summary>
    public int CruiseDateId { get; set; }

    /// <summary>
    /// Obtiene o establece el nombre del crucero.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el puerto desde el que sale el crucero.
    /// </summary>
    public string OriginPort { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la fecha de inicio del crucero.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Obtiene o establece la duración del crucero en días.
    /// </summary>
    public int DurationInDays { get; set; }
}