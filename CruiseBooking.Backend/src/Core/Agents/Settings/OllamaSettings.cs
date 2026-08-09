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

    /// <summary>
    /// Tiempo máximo de espera en segundos para las solicitudes a Ollama. Valor predeterminado: 120 segundos.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 120;

    /// <summary>
    /// Parámetro de temperatura para controlar la creatividad de las respuestas (rango 0.0 a 1.0). Valor predeterminado: 0.5.
    /// </summary>
    public float Temperature { get; init; } = 0.5f;

    /// <summary>
    /// Tamaño del contexto de la ventana para procesamiento de tokens. Valor predeterminado: 8192.
    /// </summary>
    public int NumCtx { get; init; } = 8192;
}
