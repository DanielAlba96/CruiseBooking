using Shared.Domain.Entities;
using Shared.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Shared.Persistence.Repositories;

public class CruiseRepository(IDbContextFactory<CruisesDbContext> contextFactory) : ICruiseRepository, IAsyncDisposable
{
    private readonly CruisesDbContext _context = contextFactory.CreateDbContext();

    /// <inhertidoc/>
    public IQueryable<Cruise> SearchCruises(
    string? zone = null,
        int? minDays = null,
        int? maxDays = null,
        bool adultsOnly = false,
        bool featuredOnly = false)
    {
        return ApplyCruiseFilters(_context.Cruises.AsNoTracking(), zone, minDays, maxDays, adultsOnly, featuredOnly);
    }

    /// <inhertidoc/>
    public async Task<(IReadOnlyList<Cruise> items, bool hasMore)> SearchCruisesAsync(
        string? zone = null,
        int? minDays = null,
        int? maxDays = null,
        bool adultsOnly = false,
        int pageSize = 5,
        int page = 1)
    {
        using var context = contextFactory.CreateDbContext();
        
        var items = await ApplyCruiseFilters(context.Cruises.AsNoTracking(), zone, minDays, maxDays, adultsOnly, false)
            .Include(x => x.CruiseDates)
            .Skip(pageSize * (page - 1))
            .Take(pageSize + 1) // Obtiene 1 elemento mas para saber si hay otra página
            .ToListAsync();

        bool hasMore = items.Count > pageSize;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        return (items, hasMore);
    }

    /// <inhertidoc/>
    public IQueryable<CruiseDate> GetCruiseDates(int cruiseId)
    {
        return _context.CruiseDates
            .AsNoTracking()
            .Where(cd => cd.CruiseId == cruiseId);
    }

    /// <inhertidoc/>
    public async Task<CruiseDate?> GetCruiseDateAsync(int cruiseDateId)
    {
        using var context = contextFactory.CreateDbContext();

        return await context.CruiseDates
            .AsNoTracking()
            .Include(x => x.Cruise)
            .Include(x => x.Ship)
            .FirstOrDefaultAsync(cd => cd.Id == cruiseDateId);
    }

    /// <inhertidoc/>
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
            query = query.Where(c => c.Zone.ToLower() == zone.ToLower());
        }

        if (minDays.HasValue && minDays > 0)
        {
            query = query.Where(c => c.DurationInDays >= minDays.Value);
        }

        if (maxDays.HasValue && minDays > 0)
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
