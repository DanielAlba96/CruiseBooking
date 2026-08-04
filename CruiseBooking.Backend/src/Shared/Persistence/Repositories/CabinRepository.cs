using Shared.Domain.Exceptions;
using Shared.Domain.Entities;
using Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.Persistence.Exceptions;

namespace Shared.Persistence.Repositories;

public class CabinRepository(IDbContextFactory<CruisesDbContext> contextFactory, IConfiguration configuration) : ICabinRepository, IAsyncDisposable
{
    private readonly CruisesDbContext _context = contextFactory.CreateDbContext();

    /// <summary>
    /// Obtiene las cabinas de un barco.
    /// </summary>
    public IQueryable<ShipCabin> GetShipCabins(int shipId)
    {
        return _context.ShipCabins
          .AsNoTracking()
          .Where(c => c.ShipId == shipId)
          .Select(c => new ShipCabin
          {
              Id = c.Id,
              ShipId = c.ShipId,
              CabinTypeId = c.CabinTypeId,
              Number = c.Number,
              Price = c.Price,
              CabinType = c.CabinType == null ? null : new CabinType
              {
                  Id = c.CabinType.Id,
                  Name = c.CabinType.Name,
                  Description = c.CabinType.Description,
                  MaxOccupancy = c.CabinType.MaxOccupancy
              }
          });
    }

    /// <summary>
    /// Obtiene las cabinas disponibles de una fecha de crucero.
    /// </summary>
    public async Task<IReadOnlyList<ShipCabin>> GetCabinsByCruiseDate(int cruiseDateId)
    {
        using var context = contextFactory.CreateDbContext();

        var cruiseDate = await context.CruiseDates
            .AsNoTracking()
            .Where(cd => cd.Id == cruiseDateId)
            .Select(cd => new { cd.ShipId, cd.PriceModifier })
            .FirstAsync();

        var expirationTime = GetLockExpirationTime();
        var query = context.ShipCabins.AsNoTracking().Where(c => c.ShipId == cruiseDate.ShipId);

        return await query
            .Select(c => new
            {
                Cabin = c,
                Capacity = c.Number,
                Booked = context.Bookings
                    .Where(b => b.CruiseDateId == cruiseDateId && b.Status != BookingStatus.Canceled)
                    .SelectMany(b => b.Cabins)
                    .Count(bc => bc.CabinId == c.Id),
                Locked = context.LockedCabins
                    .Where(l => l.CruiseDateId == cruiseDateId
                        && l.CabinId == c.Id
                        && l.LastUpdatedAt >= expirationTime)
                    .Count()
            })
            .Select(x => new ShipCabin
            {
                Id = x.Cabin.Id,
                Number = x.Capacity - x.Booked - x.Locked,
                Price = Math.Round(x.Cabin.Price * cruiseDate.PriceModifier, 2),
                CabinType = x.Cabin.CabinType
            }).ToListAsync();
    }

    /// <summary>
    /// Bloquea una cabina para una reserva.
    /// </summary>
    public async Task<int> LockCabin(int cruiseDateId, int userId, int cabinId, decimal price, int occupants)
    {
        using var context = contextFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        var expirationTime = GetLockExpirationTime();

        await context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({cruiseDateId}, {cabinId})");

        var cabin = await context.ShipCabins
            .Where(c => c.Id == cabinId
                && c.ShipId == context.CruiseDates
                    .Where(cd => cd.Id == cruiseDateId)
                    .Select(cd => cd.ShipId)
                    .First())
            .Select(c => new
            {
                c.Id,
                MaxOccupancy = c.CabinType!.MaxOccupancy,
                Available = c.Number
                    - context.Bookings
                        .Where(b => b.CruiseDateId == cruiseDateId && b.Status != BookingStatus.Canceled)
                        .SelectMany(b => b.Cabins)
                        .Count(bc => bc.CabinId == c.Id)
                    - context.LockedCabins
                        .Where(l => l.CruiseDateId == cruiseDateId
                            && l.CabinId == c.Id
                            && l.LastUpdatedAt >= expirationTime)
                        .Count()
            })
            .FirstOrDefaultAsync();

        if (cabin is null)
        {
            throw new CabinNotInCruiseException(cabinId);
        }

        if (cabin.Available < 1)
        {
            throw new InsufficientCabinAvailabilityException(cabinId);
        }

        // Los pasajeros reservados son el objetivo del check-in: nunca 0 ni por encima del aforo.
        if (occupants < 1 || occupants > cabin.MaxOccupancy)
        {
            throw new InvalidCabinOccupancyException(cabinId, occupants, cabin.MaxOccupancy);
        }

        var lockedCabin = new LockedCabin
        {
            CruiseDateId = cruiseDateId,
            CabinId = cabinId,
            UserId = userId,
            Occupants = occupants,
            Price = price,
            LastUpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc)
        };

        context.LockedCabins.Add(lockedCabin);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return lockedCabin.Id;
    }

    /// <summary>
    /// Desbloquea una cabina previamente bloqueada.
    /// </summary>
    public async Task UnlockCabin(int cruiseDateId, int cabinId, int userId)
    {
        using var context = contextFactory.CreateDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        await context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({cruiseDateId}, {cabinId})");

        var existing = await context.LockedCabins
            .Where(l => l.CruiseDateId == cruiseDateId && l.CabinId == cabinId && l.UserId == userId)
            .OrderBy(l => l.Id)
            .FirstOrDefaultAsync();

        if (existing is null)
        {
            return;
        }

        context.LockedCabins.Remove(existing);

        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    /// <summary>
    /// Obtiene las cabinas bloqueadas de un usuario para una fecha de crucero.
    /// </summary>
    public async Task<IReadOnlyList<LockedCabin>> GetLockedCabins(int cruiseDateId, int userId)
    {
        using var context = contextFactory.CreateDbContext();

        var expirationTime = GetLockExpirationTime();

        var cabins = await context.LockedCabins
            .Where(l => l.CruiseDateId == cruiseDateId && l.UserId == userId && l.LastUpdatedAt >= expirationTime)
            .ToListAsync();

        foreach (var c in cabins)
        {
            c.IsBookingInProgress = true;
            context.Update(c);
        }

        await context.SaveChangesAsync();

        return cabins;
    }

    /// <summary>
    /// Limpia las cabinas bloqueadas de un usuario para una fecha de crucero.
    /// </summary>
    public async Task ClearLockedCabins(int cruiseDateId, int userId)
    {
        using var context = contextFactory.CreateDbContext();

        await context.LockedCabins
            .Where(l => l.CruiseDateId == cruiseDateId && l.UserId == userId)
            .ExecuteDeleteAsync();
    }

    /// <summary>
    /// Limpia múltiples cabinas bloqueadas por sus identificadores.
    /// </summary>
    public async Task ClearLockedCabinsBulk(IReadOnlyList<int> lockedCabinsIds, CancellationToken cancellationToken)
    {
        using var context = contextFactory.CreateDbContext();

        await context.LockedCabins
            .Where(l => lockedCabinsIds.Contains(l.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Libera los recursos asincronamente.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }

    private DateTime GetLockExpirationTime()
    {
        var expirationMinutes = int.TryParse(configuration["LockedCabins:ExpirationMinutes"], out var minutes) ? minutes : 10;
        return DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc).AddMinutes(-expirationMinutes);
    }
}
