namespace CruiseBooking.Services.Api;

/// <summary>
/// Representa el resultado de una llamada a la API sin valor de retorno.
/// </summary>
public class ApiResult
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ApiResult"/>.
    /// </summary>
    protected ApiResult(bool isSuccess, string? message, int? statusCode)
    {
        IsSuccess = isSuccess;
        ErrorMessage = message;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Obtiene un valor que indica si la llamada a la API fue exitosa.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Obtiene el mensaje de error si la llamada a la API falló.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Obtiene el código de estado HTTP de la respuesta.
    /// </summary>
    public int? StatusCode { get; }

    /// <summary>
    /// Obtiene un valor que indica si la respuesta fue un estado 401 No autorizado.
    /// </summary>
    public bool IsUnauthorized => StatusCode == 401;

    /// <summary>
    /// Crea un resultado exitoso de la API.
    /// </summary>
    /// <param name="statusCode">El código de estado HTTP de la respuesta.</param>
    /// <returns>Un resultado de API que indica éxito.</returns>
    public static ApiResult Success(int? statusCode = null) => new(true, null, statusCode);

    /// <summary>
    /// Crea un resultado de error de la API.
    /// </summary>
    /// <param name="message">El mensaje de error que describe la falla.</param>
    /// <param name="statusCode">El código de estado HTTP de la respuesta.</param>
    /// <returns>Un resultado de API que indica falla.</returns>
    public static ApiResult Failure(string? message, int? statusCode) => new(false, message, statusCode);
}

/// <summary>
/// Representa el resultado de una llamada a la API que devuelve un valor.
/// </summary>
/// <typeparam name="T">El tipo del valor devuelto por la llamada a la API.</typeparam>
public sealed class ApiResult<T> : ApiResult
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ApiResult{T}"/>.
    /// </summary>
    ApiResult(bool isSuccess, T? value, string? message, int? statusCode)
        : base(isSuccess, message, statusCode) => Value = value;

    /// <summary>
    /// Obtiene el valor devuelto por la llamada a la API si fue exitosa.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Crea un resultado exitoso de la API con un valor.
    /// </summary>
    /// <param name="value">El valor devuelto por la llamada a la API.</param>
    /// <param name="statusCode">El código de estado HTTP de la respuesta.</param>
    /// <returns>Un resultado de API que contiene el valor e indica éxito.</returns>
    public static ApiResult<T> Success(T value, int? statusCode = null) => new(true, value, null, statusCode);

    /// <summary>
    /// Crea un resultado de error de la API.
    /// </summary>
    /// <param name="message">El mensaje de error que describe la falla.</param>
    /// <param name="statusCode">El código de estado HTTP de la respuesta.</param>
    /// <returns>Un resultado de API que indica falla sin valor.</returns>
    public static new ApiResult<T> Failure(string? message, int? statusCode) => new(false, default, message, statusCode);
}
