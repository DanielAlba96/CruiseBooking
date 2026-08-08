using Markdig;
using Microsoft.AspNetCore.Components;

namespace CruiseBooking.Components.Shared;

/// <summary>
/// Convierte contenido markdown en HTML listo para renderizarse en componentes Blazor.
/// </summary>
public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline s_pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    /// <summary>
    /// Convierte el markdown indicado en un <see cref="MarkupString"/> con el HTML resultante.
    /// </summary>
    /// <param name="content">Contenido markdown a convertir; admite nulo.</param>
    /// <returns>El HTML renderizado, o una cadena vacía si <paramref name="content"/> es nulo.</returns>
    public static MarkupString ToMarkupString(string? content) =>
        new(Markdown.ToHtml(content ?? string.Empty, s_pipeline));
}
