namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa la información de check-in para una reserva.
/// </summary>
/// <param name="BookingId">El identificador único de la reserva.</param>
/// <param name="CruiseName">El nombre del crucero.</param>
/// <param name="CruiseDescription">La descripción del crucero.</param>
/// <param name="Origin">El puerto de origen del crucero.</param>
/// <param name="StartDate">La fecha de inicio del crucero.</param>
/// <param name="EndDate">La fecha de finalización del crucero.</param>
/// <param name="Cabins">Los camarotes asociados a esta reserva para el check-in.</param>
public record GetCheckInResponse(
    int BookingId,
    string CruiseName,
    string CruiseDescription,
    string Origin,
    DateTime StartDate,
    DateTime EndDate,
    List<CheckInCabinResponse> Cabins
);

/// <summary>
/// Representa la información de check-in de un camarote.
/// </summary>
/// <param name="BookingCabinId">El identificador único del camarote reservado.</param>
/// <param name="CabinType">El tipo o categoría del camarote.</param>
/// <param name="CabinDescription">La descripción del camarote.</param>
/// <param name="Occupancy">El número de ocupantes en el camarote.</param>
/// <param name="Passengers">Los pasajeros asignados a este camarote.</param>
public record CheckInCabinResponse(
    int BookingCabinId,
    string CabinType,
    string CabinDescription,
    int Occupancy,
    List<CheckInPassengerResponse> Passengers
);

/// <summary>
/// Representa la información de check-in de un pasajero.
/// </summary>
/// <param name="Id">El identificador único del registro de pasajero en check-in.</param>
/// <param name="Name">El nombre del pasajero.</param>
/// <param name="Surname">El apellido del pasajero.</param>
/// <param name="BirthDate">La fecha de nacimiento del pasajero.</param>
/// <param name="Dni">El DNI (Documento Nacional de Identidad) del pasajero.</param>
/// <param name="Email">La dirección de correo electrónico del pasajero.</param>
/// <param name="Phone">El número de teléfono del pasajero.</param>
/// <param name="Address">La dirección del pasajero.</param>
public record CheckInPassengerResponse(
    int Id,
    string Name,
    string Surname,
    DateTime BirthDate,
    string Dni,
    string Email,
    string Phone,
    string Address
);

/// <summary>
/// Representa la respuesta después de agregar un pasajero durante el check-in.
/// </summary>
/// <param name="PassengerId">El identificador único del pasajero recién agregado.</param>
public record AddCheckInPassengerResponse(int PassengerId);
