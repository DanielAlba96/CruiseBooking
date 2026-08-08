using Shared.Domain.Entities;
using Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Shared.Persistence.Repositories;

public class CruiseRepository(IDbContextFactory<CruisesDbContext> contextFactory) : ICruiseRepository, IAsyncDisposable
{
    private readonly CruisesDbContext _context = contextFactory.CreateDbContext();

    /// <summary>Busca cruceros con filtros opcionales.</summary>
    public IQueryable<Cruise> SearchCruises(
        string? zone = null,
        int? minDays = null,
        int? maxDays = null,
        bool adultsOnly = false,
        bool featuredOnly = false)
    {
        return ApplyCruiseFilters(_context.Cruises.AsNoTracking(), zone, minDays, maxDays, adultsOnly, featuredOnly);
    }

    /// <summary>Busca cruceros de forma asíncrona con filtros opcionales.</summary>
    public async Task<IReadOnlyList<Cruise>> SearchCruisesAsync(
        string? zone = null,
        int? minDays = null,
        int? maxDays = null,
        bool adultsOnly = false,
        bool featuredOnly = false)
    {
        using var context = contextFactory.CreateDbContext();

        return await ApplyCruiseFilters(context.Cruises.AsNoTracking(), zone, minDays, maxDays, adultsOnly, featuredOnly)
            .ToListAsync();
    }

    /// <summary>Obtiene las fechas de un crucero.</summary>
    public IQueryable<CruiseDate> GetCruiseDates(int cruiseId)
    {
        return _context.CruiseDates
            .AsNoTracking()
            .Where(cd => cd.CruiseId == cruiseId);
    }

    /// <summary>Obtiene las fechas de un crucero de forma asíncrona.</summary>
    public async Task<IReadOnlyList<CruiseDate>> GetCruiseDatesAsync(int cruiseId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.CruiseDates
            .AsNoTracking()
            .Where(cd => cd.CruiseId == cruiseId)
            .ToListAsync();
    }

    /// <summary>Obtiene una fecha de crucero específica de forma asíncrona.</summary>
    public async Task<CruiseDate?> GetCruiseDateAsync(int cruiseDateId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.CruiseDates
            .AsNoTracking()
            .Include(x => x.Cruise)
            .Include(x => x.Ship)
            .FirstOrDefaultAsync(cd => cd.Id == cruiseDateId);
    }

    /// <summary>Obtiene una fecha de crucero con sus extras incluidos.</summary>
    public async Task<CruiseDate> GetCruiseDateWithExtras(int cruiseDateId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.CruiseDates
            .AsNoTracking()
            .Where(cd => cd.Id == cruiseDateId)
            .Select(cd => new CruiseDate
            {
                Id = cd.Id,
                ShipId = cd.ShipId,
                PriceModifier = cd.PriceModifier,
                StartDate = cd.StartDate,
                Cruise = cd.Cruise,
                CruiseDateExtras = cd.CruiseDateExtras
                    .Select(e => new CruiseDateExtra
                    {
                        ExtraId = e.ExtraId,
                        Price = e.Price,
                        Extra = e.Extra
                    }).ToList()
            })
            .FirstAsync();
    }

    private static IQueryable<Cruise> ApplyCruiseFilters(
        IQueryable<Cruise> query,
        string? zone,
        int? minDays,
        int? maxDays,
        bool adultsOnly,
        bool featuredOnly)
    {
        if (!string.IsNullOrEmpty(zone))
        {
            query = query.Where(c => c.Zone == zone);
        }

        if (minDays.HasValue)
        {
            query = query.Where(c => c.DurationInDays >= minDays.Value);
        }

        if (maxDays.HasValue)
        {
            query = query.Where(c => c.DurationInDays <= maxDays.Value);
        }

        if (adultsOnly)
        {
            query = query.Where(c => c.AdultsOnly == adultsOnly);
        }

        if (featuredOnly)
        {
            query = query.Where(c => c.Featured == featuredOnly);
        }

        return query;
    }

    /// <summary>Obtiene los cruceros destacados.</summary>
    public IQueryable<Cruise> GetFeaturedCruises()
    {
        return _context.Cruises
            .AsNoTracking()
            .Where(x => x.Featured)
            .Select(x => new Cruise
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name
            });
    }

    /// <summary>Obtiene los puertos de origen de todos los cruceros.</summary>
    public IQueryable<string> GetCruisePorts()
    {
        return _context.Cruises
            .AsNoTracking()
            .Select(c => c.OriginPort)
            .Distinct();
    }

    /// <summary>Libera los recursos del contexto de la base de datos.</summary>
    public ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }
}
