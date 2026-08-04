using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain.Exceptions;

namespace Core.Api.Common;

/// <summary>
/// Gestiona y traduce cualquier <see cref="Exception"/> a una respuesta <see cref="ProblemDetails"/>
/// </summary>
/// <param name="logger">Logger usado para registrar el fallo.</param>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        IResult resultWithProblem;
        if (exception is ControlledException controlledException)
        {
            resultWithProblem = Results.Problem(statusCode: controlledException.StatusCode,
                title: "Error controlado",
                detail: exception.Message);

            _logger.LogWarning(
                controlledException,
                "Fallo controlado {StatusCode} en {Method} {Path}.",
                controlledException.StatusCode,
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            resultWithProblem = Results.Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Error no controlado",
                detail: "Ha ocurrido un error en el servidor, intentelo de nuevo más tarde");

            _logger.LogError(
                exception,
                "Fallo no controlado en {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        await resultWithProblem.ExecuteAsync(httpContext);

        return true;
    }
}
