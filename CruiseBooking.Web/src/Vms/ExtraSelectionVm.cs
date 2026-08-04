namespace CruiseBooking.Vms;

/// <summary>
/// Representa un servicio adicional seleccionado por el usuario durante la reserva.
/// </summary>
public class ExtraSelectionVm
{
    /// <summary>
    /// Obtiene o establece el identificador único del extra.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Obtiene o establece el nombre del extra.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la descripción del extra.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece el precio del extra.
    /// </summary>
    public decimal Price { get; set; }
}