using Core.Application.Common;
using Core.Application.Common.Exceptions;
using Core.Application.Models;
using Core.Application.Resources;
using Shared.Domain.Entities;
using Shared.Domain.Repositories;
using Shared.Domain.Services;

namespace Core.Application.UseCases.CheckIn.Common;

/// <summary>
/// Lógica común a los casos de uso de check-in. Cada operación se expone por dos vías de acceso
/// (token opaco enviado por correo y reserva del usuario autenticado) que solo difieren en cómo se
/// localiza la reserva, por lo que la resolución y el resto de pasos se comparten aquí.
/// </summary>
internal static class CheckInHelpers
{
    /// <summary>Localiza la reserva a partir del token de check-in y comprueba que el check-in siga disponible.</summary>
    /// <param name="bookingRepository">Repositorio de reservas.</param>
    /// <param name="token">Token opaco de check-in enviado por correo.</param>
    /// <returns>La reserva asociada al token.</returns>
    /// <exception cref="CheckInTokenNotFoundException">El token no tiene un formato válido o no corresponde a ninguna reserva.</exception>
    internal static async Task<Booking> LoadByToken(IBookingRepository bookingRepository, string token)
    {
        if (!CheckInTokens.HasValidFormat(token))
        {
            throw new CheckInTokenNotFoundException();
        }

        var booking = await bookingRepository.GetBookingByCheckInTokenHash(CheckInTokens.ComputeHash(token))
            ?? throw new CheckInTokenNotFoundException();

        EnsureCheckInAvailable(booking);

        return booking;
    }

    /// <summary>Localiza la reserva del usuario autenticado y comprueba que el check-in siga disponible.</summary>
    /// <param name="bookingRepository">Repositorio de reservas.</param>
    /// <param name="bookingId">Identificador de la reserva.</param>
    /// <param name="userId">Identificador del usuario propietario de la reserva.</param>
    /// <returns>La reserva solicitada.</returns>
    /// <exception cref="NotFoundException">La reserva no existe o no pertenece al usuario.</exception>
    internal static async Task<Booking> LoadByOwner(IBookingRepository bookingRepository, int bookingId, int userId)
    {
        var booking = await bookingRepository.GetBookingByIdAndUserId(bookingId, userId)
            ?? throw new NotFoundException(ErrorMessages.BookingNotFoundOrNotOwned);

        EnsureCheckInAvailable(booking);

        return booking;
    }

    /// <summary>Proyecta la reserva a la vista de check-in.</summary>
    /// <param name="booking">Reserva a proyectar.</param>
    /// <returns>Los datos de check-in de la reserva.</returns>
    internal static GetCheckInDtoResponse MapCheckIn(Booking booking)
    {
        var cruise = booking.CruiseDate?.Cruise;

        return new GetCheckInDtoResponse
        {
            BookingId = booking.Id,
            CruiseName = cruise?.Name ?? string.Empty,
            CruiseDescription = cruise?.Description ?? string.Empty,
            Origin = cruise?.OriginPort ?? string.Empty,
            StartDate = booking.CruiseDate?.StartDate ?? DateTime.MinValue,
            EndDate = (booking.CruiseDate?.StartDate ?? DateTime.MinValue).AddDays(cruise?.DurationInDays ?? 0),
            Cabins = [.. booking.Cabins.Select(bc => new GetCheckInCabinDtoResponse
            {
                BookingCabinId = bc.Id,
                CabinType = bc.Cabin?.CabinType?.Name ?? string.Empty,
                CabinDescription = bc.Cabin?.CabinType?.Description ?? string.Empty,
                Occupancy = bc.Occupants,
                Passengers = [.. bc.OccupantsDetails.Select(o => new GetCheckInPassengerDtoResponse
                {
                    Id = o.Id,
                    Name = o.Name,
                    Surname = o.Surname,
                    BirthDate = o.BirthDate,
                    Dni = o.Dni,
                    Email = o.Email,
                    Phone = o.Phone,
                    Address = o.Address
                })]
            })]
        };
    }

    /// <summary>Registra un pasajero en un camarote de la reserva.</summary>
    /// <param name="bookingRepository">Repositorio de reservas.</param>
    /// <param name="booking">Reserva sobre la que se hace el check-in.</param>
    /// <param name="bookingCabinId">Identificador del camarote de la reserva.</param>
    /// <param name="passenger">Datos del pasajero.</param>
    /// <returns>El identificador del pasajero registrado.</returns>
    /// <exception cref="NotFoundException">El camarote no pertenece a la reserva.</exception>
    /// <exception cref="ValidationException">El camarote ya está al completo o los datos del pasajero no son válidos.</exception>
    /// <exception cref="CheckInNoLongerAvailableException">El check-in ha dejado de estar disponible mientras se registraba el pasajero.</exception>
    internal static async Task<int> AddPassenger(
        IBookingRepository bookingRepository,
        Booking booking,
        int bookingCabinId,
        BookingCabinOccupantRequest passenger)
    {
        var cabin = booking.Cabins.FirstOrDefault(bc => bc.Id == bookingCabinId)
            ?? throw new NotFoundException(ErrorMessages.CabinNotInBooking);

        // El límite es siempre la ocupación reservada del camarote, no el aforo del tipo de camarote.
        if (cabin.OccupantsDetails.Count >= cabin.Occupants)
        {
            throw new ValidationException(string.Format(ErrorMessages.CabinBookedOccupancyReached, cabin.Occupants));
        }

        ValidateCheckInPassenger(passenger);
        ValidateDniIsUnique(booking, passenger.Dni);

        var occupant = MapCheckInPassenger(passenger, bookingCabinId);

        return await bookingRepository.AddCheckInPassenger(booking.Id, occupant)
            ?? throw new CheckInNoLongerAvailableException();
    }

    /// <summary>Elimina un pasajero de la reserva.</summary>
    /// <param name="bookingRepository">Repositorio de reservas.</param>
    /// <param name="booking">Reserva sobre la que se hace el check-in.</param>
    /// <param name="passengerId">Identificador del pasajero a eliminar.</param>
    /// <exception cref="NotFoundException">El pasajero no pertenece a la reserva.</exception>
    /// <exception cref="CheckInNoLongerAvailableException">El check-in ha dejado de estar disponible.</exception>
    internal static async Task RemovePassenger(IBookingRepository bookingRepository, Booking booking, int passengerId)
    {
        FindCheckInPassenger(booking, passengerId);

        if (!await bookingRepository.RemoveCheckInPassenger(booking.Id, passengerId))
        {
            throw new CheckInNoLongerAvailableException();
        }
    }

    /// <summary>Cierra el check-in de la reserva y notifica el evento correspondiente.</summary>
    /// <param name="bookingRepository">Repositorio de reservas.</param>
    /// <param name="workflowService">Servicio de workflows, usado para notificar el check-in completado.</param>
    /// <param name="booking">Reserva sobre la que se hace el check-in.</param>
    /// <exception cref="ValidationException">Falta algún pasajero por registrar.</exception>
    /// <exception cref="CheckInNoLongerAvailableException">El check-in ha dejado de estar disponible.</exception>
    internal static async Task Complete(
        IBookingRepository bookingRepository,
        IWorkflowService workflowService,
        Booking booking)
    {
        if (booking.Cabins.Any(bc => bc.OccupantsDetails.Count < bc.Occupants))
        {
            throw new ValidationException(ErrorMessages.MissingPassengers);
        }

        if (!await bookingRepository.CompleteCheckIn(booking.Id))
        {
            throw new CheckInNoLongerAvailableException();
        }

        await workflowService.SendCheckInCompletedEvent(booking.Id);
    }

    /// <summary>Comprueba que la reserva admite operaciones de check-in en este momento.</summary>
    /// <param name="booking">Reserva a comprobar.</param>
    /// <exception cref="BookingPaymentPendingException">La reserva todavía no está pagada.</exception>
    /// <exception cref="CheckInNotStartedException">El check-in todavía no ha abierto.</exception>
    /// <exception cref="CheckInNoLongerAvailableException">La reserva está cancelada, ya completada, o el crucero ya ha salido.</exception>
    private static void EnsureCheckInAvailable(Booking booking)
    {
        if (booking.Status is BookingStatus.Created or BookingStatus.PendingPayment)
        {
            throw new BookingPaymentPendingException();
        }

        if (DateTime.UtcNow < (booking.CruiseDate?.CheckInStartDate ?? DateTime.MinValue))
        {
            throw new CheckInNotStartedException();
        }

        if (booking.Status == BookingStatus.Canceled
            || booking.Status == BookingStatus.Complete
            || (booking.CruiseDate?.StartDate ?? DateTime.MinValue) <= DateTime.UtcNow)
        {
            throw new CheckInNoLongerAvailableException();
        }
    }

    /// <summary>Localiza un pasajero dentro de la reserva.</summary>
    /// <param name="booking">Reserva sobre la que se busca.</param>
    /// <param name="passengerId">Identificador del pasajero.</param>
    /// <returns>El pasajero encontrado.</returns>
    /// <exception cref="NotFoundException">El pasajero no pertenece a la reserva.</exception>
    private static BookingCabinOccupant FindCheckInPassenger(Booking booking, int passengerId)
    {
        return booking.Cabins
            .SelectMany(bc => bc.OccupantsDetails)
            .FirstOrDefault(o => o.Id == passengerId)
            ?? throw new NotFoundException(ErrorMessages.PassengerNotInBooking);
    }

    /// <summary>Valida los datos obligatorios del pasajero.</summary>
    /// <param name="passenger">Datos del pasajero.</param>
    /// <exception cref="ValidationException">Falta algún campo obligatorio o la fecha de nacimiento no es válida.</exception>
    private static void ValidateCheckInPassenger(BookingCabinOccupantRequest passenger)
    {
        var allFieldsPresent = !string.IsNullOrWhiteSpace(passenger.Name)
            && !string.IsNullOrWhiteSpace(passenger.Surname)
            && !string.IsNullOrWhiteSpace(passenger.Dni)
            && !string.IsNullOrWhiteSpace(passenger.Email)
            && !string.IsNullOrWhiteSpace(passenger.Phone)
            && !string.IsNullOrWhiteSpace(passenger.Address);

        if (!allFieldsPresent)
        {
            throw new ValidationException(ErrorMessages.PassengerFieldsRequired);
        }

        if (passenger.BirthDate == default || passenger.BirthDate > DateTime.UtcNow)
        {
            throw new ValidationException(ErrorMessages.PassengerBirthDateInvalid);
        }
    }

    /// <summary>Comprueba que el DNI no esté ya registrado en la reserva.</summary>
    /// <param name="booking">Reserva sobre la que se comprueba.</param>
    /// <param name="dni">DNI del pasajero.</param>
    /// <exception cref="ValidationException">Ya hay un pasajero con ese DNI en la reserva.</exception>
    private static void ValidateDniIsUnique(Booking booking, string dni)
    {
        var normalized = dni.Trim().ToUpperInvariant();
        var duplicated = booking.Cabins
            .SelectMany(bc => bc.OccupantsDetails)
            .Any(o => o.Dni.Trim().ToUpperInvariant() == normalized);

        if (duplicated)
        {
            throw new ValidationException(string.Format(ErrorMessages.DuplicatePassengerDni, dni.Trim()));
        }
    }

    /// <summary>Convierte los datos recibidos del pasajero en la entidad de ocupante del camarote.</summary>
    /// <param name="passenger">Datos del pasajero.</param>
    /// <param name="bookingCabinId">Identificador del camarote de la reserva.</param>
    /// <returns>El ocupante listo para persistir.</returns>
    private static BookingCabinOccupant MapCheckInPassenger(BookingCabinOccupantRequest passenger, int bookingCabinId)
    {
        return new BookingCabinOccupant
        {
            BookingCabinId = bookingCabinId,
            Name = passenger.Name.Trim(),
            Surname = passenger.Surname.Trim(),
            BirthDate = DateTime.SpecifyKind(passenger.BirthDate, DateTimeKind.Utc),
            Dni = passenger.Dni.Trim(),
            Email = passenger.Email.Trim(),
            Phone = passenger.Phone.Trim(),
            Address = passenger.Address.Trim()
        };
    }
}
