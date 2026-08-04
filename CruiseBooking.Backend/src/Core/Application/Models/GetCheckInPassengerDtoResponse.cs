namespace Core.Application.Models;

/// <summary>Pasajero ya registrado en el check-in online, con su identificador para editarlo o eliminarlo.</summary>
public class GetCheckInPassengerDtoResponse
{
    /// <summary>Identificador único del pasajero.</summary>
    /// <value>Número entero que identifica el pasajero.</value>
    public int Id { get; set; }

    /// <summary>Nombre del pasajero.</summary>
    /// <value>Cadena de texto con el nombre.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>Apellido del pasajero.</summary>
    /// <value>Cadena de texto con el apellido.</value>
    public string Surname { get; set; } = string.Empty;

    /// <summary>Fecha de nacimiento del pasajero.</summary>
    /// <value>Fecha en formato UTC.</value>
    public DateTime BirthDate { get; set; }

    /// <summary>Número de documento de identidad del pasajero.</summary>
    /// <value>Cadena de texto con el DNI.</value>
    public string Dni { get; set; } = string.Empty;

    /// <summary>Correo electrónico del pasajero.</summary>
    /// <value>Cadena de texto con la dirección de correo.</value>
    public string Email { get; set; } = string.Empty;

    /// <summary>Número de teléfono del pasajero.</summary>
    /// <value>Cadena de texto con el teléfono de contacto.</value>
    public string Phone { get; set; } = string.Empty;

    /// <summary>Dirección del domicilio del pasajero.</summary>
    /// <value>Cadena de texto con la dirección completa.</value>
    public string Address { get; set; } = string.Empty;
}
