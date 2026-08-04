namespace Jobs.Application.Common.Exceptions;

/// <summary>
/// Indica un fallo permanente al enviar un correo (por ejemplo, destinatario inválido o rechazado por el servidor),
/// que no debe reintentarse.
/// </summary>
public sealed class PermanentEmailException : Exception
{
    /// <summary>Crea la excepción con un mensaje descriptivo.</summary>
    /// <param name="message">Descripción del fallo permanente.</param>
    public PermanentEmailException(string message) : base(message)
    {
    }

    /// <summary>Crea la excepción con un mensaje descriptivo y la excepción de origen.</summary>
    /// <param name="message">Descripción del fallo permanente.</param>
    /// <param name="innerException">Excepción que originó el fallo.</param>
    public PermanentEmailException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
