using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Core.Api.Common;

/// <summary>Adds the Bearer JWT security scheme to the generated OpenAPI document so Scalar can prompt for and attach a token.</summary>
/// <param name="authSchemeProvider">Provides the authentication schemes registered in the application.</param>
internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authSchemeProvider) : IOpenApiDocumentTransformer
{
    readonly IAuthenticationSchemeProvider _authSchemeProvider = authSchemeProvider;

    /// <summary>Registers the Bearer security scheme on the document and requires it on every operation, when JWT Bearer authentication is configured.</summary>
    /// <param name="document">The OpenAPI document being generated.</param>
    /// <param name="context">Contextual information about the document being transformed.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var schemes = await _authSchemeProvider.GetAllSchemesAsync();

        if (!schemes.Any(scheme => scheme.Name == "Bearer"))
        {
            return;
        }

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            }
        };

        foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations!))
        {
            operation.Value.Security ??= [];
            operation.Value.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        }
    }
}
