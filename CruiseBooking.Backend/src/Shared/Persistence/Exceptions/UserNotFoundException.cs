using Shared.Domain.Exceptions;
using Shared.Persistence.Resources;

namespace Shared.Persistence.Exceptions;

/// <summary>Se lanza cuando no existe en la base de datos el usuario solicitado.</summary>
public sealed class UserNotFoundException()
    : ControlledException(ErrorMessages.UserNotFound)
{
    /// <inheritdoc />
    public override int StatusCode => 404;
}
