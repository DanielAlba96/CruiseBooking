using Shared.Domain.Exceptions;

namespace Core.Application.Common.Exceptions;

/// <summary>Se lanza cuando los datos enviados por el cliente no cumplen las reglas de negocio.</summary>
/// <param name="message">Descripción del dato inválido, apta para mostrarse al cliente.</param>
public sealed class ValidationException(string message)
    : ControlledException(message)
{
    /// <inheritdoc />
    public override int StatusCode => 400;
}
