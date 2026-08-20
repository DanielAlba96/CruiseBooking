using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Shared.Persistence;

/// <summary>
/// Contexto de base de datos Entity Framework Core para la gestión de reservas de cruceros.
/// Proporciona acceso a todas las entidades del sistema: barcos, cruceros, camarotes, reservas y usuarios.
/// </summary>
public class CruisesDbContext(DbContextOptions<CruisesDbContext> options) : DbContext(options)
{
    readonly DbContextOptions<CruisesDbContext> _options = options;

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de barcos.
    /// </summary>
    /// <value>
    /// Colección de barcos disponibles en el sistema.
    /// </value>
    public DbSet<Ship> Ships { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de cruceros.
    /// </summary>
    /// <value>
    /// Colección de cruceros ofrecidos.
    /// </value>
    public DbSet<Cruise> Cruises { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de fechas de crucero.
    /// </summary>
    /// <value>
    /// Colección de fechas específicas en que se realizan los cruceros.
    /// </value>
    public DbSet<CruiseDate> CruiseDates { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de tipos de camarote.
    /// </summary>
    /// <value>
    /// Colección de categorías de camarotes disponibles.
    /// </value>
    public DbSet<CabinType> CabinTypes { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de camarotes de barco.
    /// </summary>
    /// <value>
    /// Colección de camarotes específicos en cada barco.
    /// </value>
    public DbSet<ShipCabin> ShipCabins { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de servicios adicionales.
    /// </summary>
    /// <value>
    /// Colección de extras disponibles para agregar a las reservas.
    /// </value>
    public DbSet<Extra> Extras { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de relaciones entre fechas de crucero y servicios adicionales.
    /// </summary>
    /// <value>
    /// Colección que vincula extras con fechas específicas de cruceros.
    /// </value>
    public DbSet<CruiseDateExtra> CruiseDateExtras { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de reservas.
    /// </summary>
    /// <value>
    /// Colección de reservas de cruceros realizadas por usuarios.
    /// </value>
    public DbSet<Booking> Bookings { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de camarotes asociados a reservas.
    /// </summary>
    /// <value>
    /// Colección que vincula camarotes con las reservas específicas.
    /// </value>
    public DbSet<BookingCabin> BookingCabins { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de ocupantes de camarotes en reservas.
    /// </summary>
    /// <value>
    /// Colección de personas que ocuparán los camarotes en una reserva.
    /// </value>
    public DbSet<BookingCabinOccupant> BookingCabinOccupants { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de servicios adicionales asociados a reservas.
    /// </summary>
    /// <value>
    /// Colección que vincula extras contratados con las reservas.
    /// </value>
    public DbSet<BookingExtra> BookingExtras { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de usuarios del sistema.
    /// </summary>
    /// <value>
    /// Colección de usuarios registrados en la plataforma.
    /// </value>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de métodos de pago de usuarios.
    /// </summary>
    /// <value>
    /// Colección de métodos de pago asociados a cada usuario.
    /// </value>
    public DbSet<UserPayment> PaymentMethods { get; set; }

    /// <summary>
    /// Obtiene o establece el conjunto de entidades de camarotes bloqueados.
    /// </summary>
    /// <value>
    /// Colección de camarotes que están bloqueados y no disponibles para reservas.
    /// </value>
    public DbSet<LockedCabin> LockedCabins { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CruisesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseAsyncSeeding(async (context, _, ct) =>
        {
            await DbSeeder.SeedAsync((CruisesDbContext)context, ct);
        });

        optionsBuilder.UseSeeding(async (context, _) =>
        {
            DbSeeder.Seed((CruisesDbContext)context);
        });
    }
}
