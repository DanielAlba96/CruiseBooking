namespace Jobs.Infrastructure.Options;

/// <summary>
/// Opciones del remitente de correo, enlazadas desde la sección de configuración "Email".
/// </summary>
public sealed class EmailOptions
{
    /// <summary>Nombre de la sección de configuración.</summary>
    public const string SectionName = "Email";

    /// <summary>Dirección de correo del remitente.</summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>Nombre visible del remitente.</summary>
    public string FromName { get; set; } = string.Empty;
}
