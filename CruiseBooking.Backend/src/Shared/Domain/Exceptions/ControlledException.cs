namespace Shared.Domain.Exceptions;

/// <summary>Excepción de aplicación que conoce el código HTTP con el que debe responderse.</summary>
public abstract class ControlledException : Exception
{
    /// <summary>Código de estado HTTP asociado a este fallo.</summary>
    public abstract int StatusCode { get; }

    /// <summary>Crea la excepción con un mensaje descriptivo.</summary>
    /// <param name="message">Descripción del fallo, apta para mostrarse al cliente.</param>
    protected ControlledException(string message) : base(message)
    {
    }

    /// <summary>Crea la excepción con un mensaje descriptivo y la excepción de origen.</summary>
    /// <param name="message">Descripción del fallo, apta para mostrarse al cliente.</param>
    /// <param name="innerException">Excepción que originó el fallo.</param>
    protected ControlledException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
