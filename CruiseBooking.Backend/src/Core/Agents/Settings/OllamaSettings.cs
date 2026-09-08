namespace Core.Agents.Settings;

/// <summary>
/// Opciones de configuración para la integración con Ollama.
/// </summary>
public sealed class OllamaSettings
{
    public const string SectionName = "Ollama";

    /// <summary>
    /// URL base del servidor Ollama.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Nombre del modelo de IA a utilizar en Ollama.
    /// </summary>
    public string Model { get; init; } = string.Empty;
}
