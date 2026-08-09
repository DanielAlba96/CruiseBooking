namespace Core.Agents.Settings;

internal class OpenAISettings
{
    public const string SectionName = "OpenAI";

    /// <summary>
    /// Indica si se debe usar el proveedor
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// URL base del servidor.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Nombre del modelo de IA a utilizar.
    /// </summary>
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// Clave privada para acceder a la api.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
