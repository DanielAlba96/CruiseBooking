using Shared.Domain.Exceptions;

namespace Core.Application.Common.Exceptions;

/// <summary>Se lanza cuando el recurso solicitado no existe o no es accesible para el usuario actual.</summary>
/// <param name="message">Descripción del recurso que no se ha encontrado.</param>
public class NotFoundException(string message)
    : ControlledException(message)
{
    /// <inheritdoc />
    public override int StatusCode => 404;
}
