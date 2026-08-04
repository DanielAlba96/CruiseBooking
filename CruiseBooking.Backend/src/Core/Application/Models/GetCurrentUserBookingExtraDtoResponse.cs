namespace Core.Application.Models;

/// <summary>
/// Respuesta DTO que contiene la información del extra de una reserva del usuario actual.
/// </summary>
public class GetCurrentUserBookingExtraDtoResponse
{
    /// <summary>
    /// Identificador único del extra.
    /// </summary>
    /// <value>El ID del extra en la base de datos.</value>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del extra.
    /// </summary>
    /// <value>Descripción breve del nombre del extra.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del extra.
    /// </summary>
    /// <value>Información adicional sobre el extra.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Precio del extra.
    /// </summary>
    /// <value>El costo en formato decimal.</value>
    public decimal Price { get; set; }
}
