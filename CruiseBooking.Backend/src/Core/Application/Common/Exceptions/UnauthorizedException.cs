using Shared.Domain.Exceptions;

namespace Core.Application.Common.Exceptions;

/// <summary>Se lanza cuando el usuario actual no reúne las condiciones necesarias para ejecutar la operación.</summary>
/// <param name="message">Descripción del motivo por el que se rechaza la operación.</param>
public sealed class UnauthorizedException(string message)
    : ControlledException(message)
{
    /// <inheritdoc />
    public override int StatusCode => 401;
}
