namespace Core.Infrastructure.Settings;

/// <summary>
/// Opciones de configuración para el proceso de check-in de pasajeros.
/// </summary>
public sealed class CheckInSettings
{
    public const string SectionName = "CheckIn";

    /// <summary>
    /// Obtiene o establece la URL base del frontend para generar enlaces de check-in.
    /// </summary>
    public string FrontendBaseUrl { get; set; } = string.Empty;
}
