using System.Globalization;

namespace CruiseBooking.Components.Shared;

/// <summary>
/// Formato de importes y fechas para la interfaz, con la cultura es-ES.
/// El servidor envía los valores en crudo: la presentación se decide aquí.
/// </summary>
public static class DisplayFormat
{
    /// <summary>
    /// Cultura utilizada para dar formato a importes y fechas.
    /// </summary>
    public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("es-ES");

    /// <summary>
    /// Da formato de moneda a un importe.
    /// </summary>
    /// <param name="value">Importe a formatear.</param>
    /// <returns>El importe con el símbolo de moneda de la cultura es-ES.</returns>
    public static string Price(decimal value)
    {
        return value.ToString("C", Culture);
    }

    /// <summary>
    /// Da formato de fecha corta a una fecha.
    /// </summary>
    /// <param name="value">Fecha a formatear.</param>
    /// <returns>La fecha en formato corto de la cultura es-ES.</returns>
    public static string Date(DateTime value)
    {
        return value.ToString("d", Culture);
    }
}
