using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;

namespace Core.Application.Common;

/// <summary>
/// Utilidades para generar y validar los tokens opacos de check-in online.
/// El token en crudo solo viaja en el enlace del email; en base de datos se persiste únicamente su hash.
/// </summary>
public static class CheckInTokens
{
    /// <summary>Longitud del token en caracteres: 32 bytes codificados en base64url sin relleno.</summary>
    public const int TokenLength = 43;

    /// <summary>Genera un token aleatorio criptográficamente seguro en formato base64url.</summary>
    /// <returns>Token de <see cref="TokenLength"/> caracteres.</returns>
    public static string GenerateToken()
    {
        return Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
    }

    /// <summary>Calcula el hash SHA-256 de un token, en hexadecimal minúsculas.</summary>
    /// <param name="token">Token en crudo.</param>
    /// <returns>Hash de 64 caracteres para persistir o buscar en base de datos.</returns>
    public static string ComputeHash(string token)
    {
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    /// <summary>Comprueba si una cadena tiene el formato de un token de check-in válido.</summary>
    /// <param name="token">Cadena a validar.</param>
    /// <returns><c>true</c> si la longitud y el alfabeto base64url son correctos.</returns>
    public static bool HasValidFormat(string? token)
    {
        return token is { Length: TokenLength } && Base64Url.IsValid(token);
    }
}
