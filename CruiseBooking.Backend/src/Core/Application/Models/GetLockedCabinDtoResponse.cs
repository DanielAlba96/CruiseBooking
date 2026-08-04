namespace Core.Application.Models;

/// <summary>
/// Respuesta con los datos de una camarota bloqueada.
/// </summary>
public class GetLockedCabinDtoResponse
{
    /// <summary>
    /// Identificador único de la camarota bloqueada.
    /// </summary>
    /// <value>Un entero que representa el identificador de la camarota.</value>
    public int Id { get; set; }

    /// <summary>
    /// Identificador de la camarota.
    /// </summary>
    /// <value>Un entero que representa el identificador de la camarota asociada.</value>
    public int CabinId { get; set; }

    /// <summary>
    /// Número de ocupantes.
    /// </summary>
    /// <value>Un entero que indica la cantidad de ocupantes en la camarota.</value>
    public int Occupants { get; set; }

    /// <summary>
    /// Precio de la camarota.
    /// </summary>
    /// <value>Un decimal que representa el precio en la moneda configurada.</value>
    public decimal Price { get; set; }

    /// <summary>
    /// Fecha y hora de la última actualización.
    /// </summary>
    /// <value>Un DateTime en UTC que indica cuándo se actualizó la camarota por última vez.</value>
    public DateTime LastUpdatedAt { get; set; }
}
