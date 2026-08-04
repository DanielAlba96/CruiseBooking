using Shared.Domain.Entities;
using Shared.Domain.Models;
using Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Shared.Persistence.Repositories;

public class BookingRepository(IDbContextFactory<CruisesDbContext> contextFactory) : IBookingRepository
{
    /// <summary>Crea una nueva reserva en la base de datos.</summary>
    public async Task CreateBooking(Booking booking)
    {
        using var context = contextFactory.CreateDbContext();
        await context.Bookings.AddAsync(booking);
        await context.SaveChangesAsync();
    }

    /// <summary>Marca una reserva como pagada con los datos del cargo.</summary>
    public async Task<bool> MarkBookingAsPaid(int bookingId, string chargeId, string paymentMethodId, decimal chargeAmount, DateTime chargedAt)
    {
        using var context = contextFactory.CreateDbContext();

        var updated = await context.Bookings
            .Where(b => b.Id == bookingId
                && (b.Status == BookingStatus.Created || b.Status == BookingStatus.PendingPayment)
                && b.Status != BookingStatus.Canceled)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Status, BookingStatus.PendingCheckIn)
                .SetProperty(b => b.ChargeId, chargeId)
                .SetProperty(b => b.ChargedAt, chargedAt)
                .SetProperty(b => b.ChargeAmount, chargeAmount)
                .SetProperty(b => b.PaymentMethodId, paymentMethodId));

        return updated == 1;
    }

    /// <summary>Marca el pago de una reserva como fallido.</summary>
    public async Task<bool> MarkPaymentAsFailed(int bookingId)
    {
        using var context = contextFactory.CreateDbContext();

        var updated = await context.Bookings
            .Where(b => b.Id == bookingId
                && b.Status == BookingStatus.Created
                && b.Status != BookingStatus.Canceled)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Status, BookingStatus.PendingPayment));

        return updated == 1;
    }

    /// <summary>Cancela una reserva con datos opcionales de reembolso.</summary>
    public async Task<bool> CancelBooking(int bookingId, string? refundId = null, DateTime? refundedAt = null)
    {
        using var context = contextFactory.CreateDbContext();

        var booking = await context.Bookings
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking is null)
        {
            return false;
        }

        if (booking.Status == BookingStatus.Canceled)
        {
            return true;
        }

        booking.Status = BookingStatus.Canceled;
        booking.RefundId = refundId;
        booking.RefundedAt = refundedAt;

        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>Cancela una reserva no pagada.</summary>
    public async Task<bool> CancelUnpaidBooking(int bookingId)
    {
        using var context = contextFactory.CreateDbContext();

        var updated = await context.Bookings
            .Where(b => b.Id == bookingId
                && b.Status == BookingStatus.PendingPayment)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Status, BookingStatus.Canceled));

        return updated == 1;
    }

    /// <summary>Obtiene los datos de una reserva para generar factura.</summary>
    public async Task<BookingInvoiceData?> GetBookingInvoiceData(int bookingId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new BookingInvoiceData(
                b.Id,
                b.Status == BookingStatus.Canceled,
                b.ChargedAt ?? DateTime.MinValue,
                b.ChargeAmount,
                b.User!.Name,
                b.User.Email,
                b.CruiseDate!.Cruise!.Name,
                b.CruiseDate.Cruise.Code,
                b.CruiseDate.Cruise.OriginPort,
                b.CruiseDate.Cruise.DurationInDays,
                b.CruiseDate.Cruise.Itinerary,
                b.CruiseDate.StartDate,
                b.CruiseDate.CheckInStartDate,
                b.CruiseDate.Ship!.Name,
                b.CruiseDate.Ship.Company,
                b.Cabins.Select(c => new InvoiceCabinLine(
                    c.Cabin!.CabinType!.Name,
                    c.Cabin.CabinType.Description,
                    c.Occupants,
                    c.Price)).ToList(),
                b.Extras.Select(e => new InvoiceExtraLine(
                    e.Extra!.Name,
                    e.Extra.Description,
                    e.Price)).ToList()))
            .FirstOrDefaultAsync();
    }

    /// <summary>Obtiene las fechas importantes del flujo de una reserva.</summary>
    public async Task<BookingWorkflowDates?> GetBookingWorkflowDates(int bookingId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new BookingWorkflowDates(b.CruiseDate!.PaymentStartDate, b.CruiseDate.CheckInStartDate))
            .FirstOrDefaultAsync();
    }

    /// <summary>Obtiene los datos de una reserva para enviar por correo.</summary>
    public async Task<BookingEmailData?> GetBookingEmailData(int bookingId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new BookingEmailData(
                b.Id,
                b.Status == BookingStatus.Canceled,
                b.User!.Name,
                b.User.Email,
                b.CruiseDate!.Cruise!.Name,
                b.CruiseDate.StartDate,
                b.CruiseDate.CheckInStartDate))
            .FirstOrDefaultAsync();
    }

    /// <summary>Obtiene los datos de pago de una reserva.</summary>
    public async Task<BookingPaymentData?> GetBookingPaymentData(int bookingId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId)
            .Select(b => new BookingPaymentData(
                b.Id,
                b.User!.CustomerId!,
                b.PaymentMethodId,
                b.Status == BookingStatus.Canceled,
                b.Cabins.Sum(x => x.Price) + b.Extras.Sum(x => x.Price)))
            .FirstOrDefaultAsync();
    }

    /// <summary>Obtiene todas las reservas de un usuario ordenadas por fecha de creación.</summary>
    public async Task<List<Booking>> GetBookingsByUserId(int userId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.UserId == userId)
            .Include(b => b.CruiseDate)
                .ThenInclude(cd => cd!.Cruise)
            .Include(b => b.Cabins)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    /// <summary>Obtiene una reserva específica por su ID y del usuario indicado.</summary>
    public async Task<Booking?> GetBookingByIdAndUserId(int bookingId, int userId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Bookings
            .AsNoTracking()
            .Where(b => b.Id == bookingId && b.UserId == userId)
            .Include(b => b.CruiseDate)
                .ThenInclude(cd => cd!.Cruise)
            .Include(b => b.CruiseDate)
                .ThenInclude(cd => cd!.Ship)
            .Include(b => b.Cabins)
                .ThenInclude(bc => bc.Cabin)
                    .ThenInclude(sc => sc!.CabinType)
            .Include(b => b.Cabins)
                .ThenInclude(bc => bc.OccupantsDetails)
            .Include(b => b.Extras)
                .ThenInclude(be => be.Extra)
            .FirstOrDefaultAsync();
    }

    /// <summary>Obtiene una reserva por el hash del token de check-in.</summary>
    public async Task<Booking?> GetBookingByCheckInTokenHash(string tokenHash)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Bookings
            .AsNoTracking()
            .Include(b => b.CruiseDate)
                .ThenInclude(cd => cd!.Cruise)
            .Include(b => b.CruiseDate)
                .ThenInclude(cd => cd!.Ship)
            .Include(b => b.Cabins)
                .ThenInclude(bc => bc.Cabin)
                    .ThenInclude(sc => sc!.CabinType)
            .Include(b => b.Cabins)
                .ThenInclude(bc => bc.OccupantsDetails)
            .FirstOrDefaultAsync(b => b.CheckInTokenHash == tokenHash);
    }

    /// <summary>Agrega un pasajero al check-in de una reserva.</summary>
    public async Task<int?> AddCheckInPassenger(int bookingId, BookingCabinOccupant occupant)
    {
        using var context = contextFactory.CreateDbContext();

        var cabin = await context.BookingCabins
            .FirstOrDefaultAsync(bc => bc.Id == occupant.BookingCabinId
                && bc.BookingId == bookingId
                && bc.Booking!.Status == BookingStatus.PendingCheckIn);

        if (cabin is null)
        {
            return null;
        }

        // Occupants es la ocupación reservada (y facturada) del camarote: no se toca al registrar
        // pasajeros en el check-in, es el objetivo contra el que se compara OccupantsDetails.
        context.BookingCabinOccupants.Add(occupant);
        await context.SaveChangesAsync();

        return occupant.Id;
    }

    /// <summary>Actualiza los datos de un pasajero en el check-in.</summary>
    public async Task<bool> UpdateCheckInPassenger(int bookingId, BookingCabinOccupant occupant)
    {
        using var context = contextFactory.CreateDbContext();

        var existing = await context.BookingCabinOccupants
            .FirstOrDefaultAsync(o => o.Id == occupant.Id
                && o.BookingCabin!.BookingId == bookingId
                && o.BookingCabin.Booking!.Status == BookingStatus.PendingCheckIn);

        if (existing is null)
        {
            return false;
        }

        existing.Name = occupant.Name;
        existing.Surname = occupant.Surname;
        existing.BirthDate = occupant.BirthDate;
        existing.Dni = occupant.Dni;
        existing.Email = occupant.Email;
        existing.Phone = occupant.Phone;
        existing.Address = occupant.Address;
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>Elimina un pasajero del check-in de una reserva.</summary>
    public async Task<bool> RemoveCheckInPassenger(int bookingId, int occupantId)
    {
        using var context = contextFactory.CreateDbContext();

        var existing = await context.BookingCabinOccupants
            .Include(o => o.BookingCabin)
            .FirstOrDefaultAsync(o => o.Id == occupantId
                && o.BookingCabin!.BookingId == bookingId
                && o.BookingCabin.Booking!.Status == BookingStatus.PendingCheckIn);

        if (existing is null)
        {
            return false;
        }

        context.BookingCabinOccupants.Remove(existing);
        await context.SaveChangesAsync();

        return true;
    }

    /// <summary>Marca el check-in de una reserva como completado.</summary>
    public async Task<bool> CompleteCheckIn(int bookingId)
    {
        using var context = contextFactory.CreateDbContext();

        var updated = await context.Bookings
            .Where(b => b.Id == bookingId
                && b.Status == BookingStatus.PendingCheckIn
                && b.Cabins.All(c => c.OccupantsDetails.Any()))
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Status, BookingStatus.Complete)
                .SetProperty(b => b.CheckInTokenHash, (string?)null));

        return updated == 1;
    }

    /// <summary>Establece el hash del token de check-in para una reserva.</summary>
    public async Task<bool> SetCheckInTokenHash(int bookingId, string tokenHash)
    {
        using var context = contextFactory.CreateDbContext();

        var updated = await context.Bookings
            .Where(b => b.Id == bookingId
                && b.Status == BookingStatus.PendingCheckIn)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.CheckInTokenHash, tokenHash));

        return updated == 1;
    }

    /// <summary>Verifica si existe una reserva activa para un método de pago.</summary>
    public async Task<bool> ExistsActiveBookingByPaymentMethod(string paymentMethodId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.Bookings
            .AsNoTracking()
            .AnyAsync(b => b.PaymentMethodId == paymentMethodId
                && !(b.Status == BookingStatus.Complete || b.Status == BookingStatus.Canceled));
    }
}
