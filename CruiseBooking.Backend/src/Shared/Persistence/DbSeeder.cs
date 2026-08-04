using Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Shared.Persistence;

/// <summary>
/// Proporciona métodos para sembrar la base de datos con datos iniciales de cruceros, barcos, camarotes y extras.
/// </summary>
public static class DbSeeder
{
    /// <summary>
    /// Siembra la base de datos de forma asincrónica con datos iniciales si no contiene registros de cruceros.
    /// </summary>
    /// <param name="cruisesDbContext">Contexto de la base de datos donde se añadirán los datos iniciales.</param>
    /// <param name="cancellationToken">Token de cancelación para cancelar la operación.</param>
    /// <returns>Una tarea que representa la operación asincrónica de siembra.</returns>
    public static async Task SeedAsync(CruisesDbContext cruisesDbContext, CancellationToken cancellationToken)
    {
        if (await cruisesDbContext.Cruises.AnyAsync(cancellationToken))
            return;

        var interior = new CabinType { Name = "Interior", Description = "Camarote interior sin ventanas, ideal para viajeros que buscan el mejor precio", MaxOccupancy = 2 };
        var oceanView = new CabinType { Name = "Ocean View", Description = "Camarote con ventana o portillo con vistas al mar", MaxOccupancy = 3 };
        var suite = new CabinType { Name = "Suite", Description = "Suite de lujo con sala de estar, balcón privado y servicios premium", MaxOccupancy = 4 };

        var wifi = new Extra { Code = "EX-WIFI", Name = "WiFi Ilimitado", Description = "Acceso a internet durante todo el viaje" };
        var drinks = new Extra { Code = "EX-DRINKS", Name = "Paquete Bebidas", Description = "Bebidas ilimitadas en bares seleccionados" };
        var excursion = new Extra { Code = "EX-EXCUR", Name = "Excursión", Description = "Excursión guiada en puerto" };

        var ship1 = new Ship { Code = "B01", Name = "Explorer I", Description = "Barco familiar para cruceros por el Caribe", Company = "Oceanic Co." };
        var ship2 = new Ship { Code = "B02", Name = "Luxury One", Description = "Barco premium con servicios de lujo", Company = "Luxury Cruises" };
        var ship3 = new Ship { Code = "B03", Name = "Adventure", Description = "Barco para rutas de aventura y expedición", Company = "Adventure Lines" };
        var ship4 = new Ship { Code = "B04", Name = "Pacific Star", Description = "Barco moderno para cruceros familiares por el Pacífico", Company = "Pacific Voyages" };
        var ship5 = new Ship { Code = "B05", Name = "Nordic Spirit", Description = "Barco robusto preparado para rutas del norte de Europa", Company = "Nordic Lines" };

        ship1.Cabins =
        [
            new ShipCabin { Ship = ship1, CabinType = interior, Number = 20, Price = 450m },
            new ShipCabin { Ship = ship1, CabinType = oceanView, Number = 10, Price = 650m }
        ];

        ship2.Cabins =
        [
            new ShipCabin { Ship = ship2, CabinType = suite, Number = 100, Price = 1500m },
            new ShipCabin { Ship = ship2, CabinType = oceanView, Number = 50, Price = 900m }
        ];

        ship3.Cabins =
        [
            new ShipCabin { Ship = ship3, CabinType = interior, Number = 2, Price = 400m },
            new ShipCabin { Ship = ship3, CabinType = oceanView, Number = 1, Price = 600m }
        ];

        ship4.Cabins =
        [
            new ShipCabin { Ship = ship4, CabinType = interior,  Number = 401, Price = 520m },
            new ShipCabin { Ship = ship4, CabinType = oceanView, Number = 402, Price = 750m },
            new ShipCabin { Ship = ship4, CabinType = suite,     Number = 403, Price = 1800m }
        ];

        ship5.Cabins =
        [
            new ShipCabin { Ship = ship5, CabinType = interior,  Number = 501, Price = 380m },
            new ShipCabin { Ship = ship5, CabinType = oceanView, Number = 502, Price = 580m }
        ];

        var cruise1 = new Cruise
        {
            Name = "Caribbean Explorer",
            Code = "CRU001",
            Zone = "Caribe",
            Description = "7 noches recorriendo las mejores islas del Caribe",
            OriginPort = "Miami",
            Itinerary = "Miami - Bahamas - Isla A - Isla B - Miami",
            DurationInDays = 7,
            AdultsOnly = true,
            Featured = true
        };

        var cruise2 = new Cruise
        {
            Name = "Mediterranean Luxury",
            Code = "CRU002",
            Zone = "Mediterráneo",
            Description = "9 noches por el Mediterráneo con paradas en puertos históricos",
            OriginPort = "Barcelona",
            Itinerary = "Barcelona - Marsella - Génova - Roma - Nápoles - Barcelona",
            DurationInDays = 9,
            AdultsOnly = false,
            Featured = true
        };

        var cruise3 = new Cruise
        {
            Name = "Northern Adventure",
            Code = "CRU003",
            Zone = "Atlántico Norte",
            Description = "5 noches de exploración y actividades al aire libre",
            OriginPort = "Reykjavik",
            Itinerary = "Reykjavik - Islandia - Islas - Reykjavik",
            DurationInDays = 5,
            AdultsOnly = true,
            Featured = false
        };

        var cruise4 = new Cruise
        {
            Name = "Pacific Getaway",
            Code = "CRU004",
            Zone = "Pacífico",
            Description = "14 noches descubriendo paraísos del Pacífico sur, ideal para familias",
            OriginPort = "Los Ángeles",
            Itinerary = "Los Ángeles - Honolulu - Papeete - Bora Bora - Los Ángeles",
            DurationInDays = 14,
            AdultsOnly = false,
            Featured = false
        };

        var cruise5 = new Cruise
        {
            Name = "Baltic Discovery",
            Code = "CRU005",
            Zone = "Báltico",
            Description = "10 noches navegando por los fiordos y capitales del mar Báltico",
            OriginPort = "Copenhague",
            Itinerary = "Copenhague - Oslo - Helsinki - Tallin - Estocolmo - Copenhague",
            DurationInDays = 10,
            AdultsOnly = false,
            Featured = true
        };

        var now = DateTime.UtcNow;
        var cd1Start = now.AddMonths(1);
        var cd2Start = now.AddMonths(3);
        var cd3Start = now.AddMonths(2);
        var cd4Start = now.AddMonths(1).AddDays(10);
        var cd5Start = now.AddMonths(4);
        var cd6Start = now.AddMonths(2).AddDays(5);
        var cd7Start = now.AddMonths(5);
        var cd8Start = now.AddMonths(3);

        var cd1 = new CruiseDate { Cruise = cruise1, Ship = ship1, StartDate = cd1Start, PaymentStartDate = cd1Start.AddDays(-30), CheckInStartDate = cd1Start.AddDays(-7), PriceModifier = 1.00m };
        var cd2 = new CruiseDate { Cruise = cruise1, Ship = ship1, StartDate = cd2Start, PaymentStartDate = cd2Start.AddDays(-30), CheckInStartDate = cd2Start.AddDays(-7), PriceModifier = 1.10m };

        var cd3 = new CruiseDate { Cruise = cruise2, Ship = ship2, StartDate = cd3Start, PaymentStartDate = cd3Start.AddDays(-30), CheckInStartDate = cd3Start.AddDays(-7), PriceModifier = 1.25m };

        var cd4 = new CruiseDate { Cruise = cruise3, Ship = ship3, StartDate = cd4Start, PaymentStartDate = cd4Start.AddDays(-30), CheckInStartDate = cd4Start.AddDays(-7), PriceModifier = 0.95m };
        var cd5 = new CruiseDate { Cruise = cruise3, Ship = ship3, StartDate = cd5Start, PaymentStartDate = cd5Start.AddDays(-30), CheckInStartDate = cd5Start.AddDays(-7), PriceModifier = 1.05m };
        var cd6 = new CruiseDate { Cruise = cruise4, Ship = ship4, StartDate = cd6Start, PaymentStartDate = cd6Start.AddDays(-30), CheckInStartDate = cd6Start.AddDays(-7), PriceModifier = 1.15m };
        var cd7 = new CruiseDate { Cruise = cruise4, Ship = ship4, StartDate = cd7Start, PaymentStartDate = cd7Start.AddDays(-30), CheckInStartDate = cd7Start.AddDays(-7), PriceModifier = 1.00m };
        var cd8 = new CruiseDate { Cruise = cruise5, Ship = ship5, StartDate = cd8Start, PaymentStartDate = cd8Start.AddDays(-30), CheckInStartDate = cd8Start.AddDays(-7), PriceModifier = 1.20m };

        var cde1 = new CruiseDateExtra { CruiseDate = cd1, Extra = wifi, Price = 25m };
        var cde2 = new CruiseDateExtra { CruiseDate = cd1, Extra = drinks, Price = 120m };
        var cde3 = new CruiseDateExtra { CruiseDate = cd3, Extra = excursion, Price = 80m };
        var cde4 = new CruiseDateExtra { CruiseDate = cd6, Extra = wifi, Price = 25m };
        var cde5 = new CruiseDateExtra { CruiseDate = cd6, Extra = drinks, Price = 130m };
        var cde6 = new CruiseDateExtra { CruiseDate = cd8, Extra = excursion, Price = 95m };

        IEnumerable<object> toAdd =
        [
            interior, oceanView, suite,
            wifi, drinks, excursion,
            ship1, ship2, ship3, ship4, ship5,
            cruise1, cruise2, cruise3, cruise4, cruise5,
            cd1, cd2, cd3, cd4, cd5, cd6, cd7, cd8,
            cde1, cde2, cde3, cde4, cde5, cde6
        ];

        await cruisesDbContext.AddRangeAsync(toAdd, cancellationToken);
        await cruisesDbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Siembra la base de datos de forma sincrónica con datos iniciales si no contiene registros de cruceros.
    /// </summary>
    /// <param name="cruisesDbContext">Contexto de la base de datos donde se añadirán los datos iniciales.</param>
    public static void Seed(CruisesDbContext cruisesDbContext)
    {
        if (cruisesDbContext.Cruises.Any())
            return;

        var interior = new CabinType { Name = "Interior", Description = "Camarote interior sin ventanas, ideal para viajeros que buscan el mejor precio", MaxOccupancy = 2 };
        var oceanView = new CabinType { Name = "Ocean View", Description = "Camarote con ventana o portillo con vistas al mar", MaxOccupancy = 3 };
        var suite = new CabinType { Name = "Suite", Description = "Suite de lujo con sala de estar, balcón privado y servicios premium", MaxOccupancy = 4 };

        var wifi = new Extra { Code = "EX-WIFI", Name = "WiFi Ilimitado", Description = "Acceso a internet durante todo el viaje" };
        var drinks = new Extra { Code = "EX-DRINKS", Name = "Paquete Bebidas", Description = "Bebidas ilimitadas en bares seleccionados" };
        var excursion = new Extra { Code = "EX-EXCUR", Name = "Excursión", Description = "Excursión guiada en puerto" };

        var ship1 = new Ship { Code = "B01", Name = "Explorer I", Description = "Barco familiar para cruceros por el Caribe", Company = "Oceanic Co." };
        var ship2 = new Ship { Code = "B02", Name = "Luxury One", Description = "Barco premium con servicios de lujo", Company = "Luxury Cruises" };
        var ship3 = new Ship { Code = "B03", Name = "Adventure", Description = "Barco para rutas de aventura y expedición", Company = "Adventure Lines" };
        var ship4 = new Ship { Code = "B04", Name = "Pacific Star", Description = "Barco moderno para cruceros familiares por el Pacífico", Company = "Pacific Voyages" };
        var ship5 = new Ship { Code = "B05", Name = "Nordic Spirit", Description = "Barco robusto preparado para rutas del norte de Europa", Company = "Nordic Lines" };

        ship1.Cabins =
        [
            new ShipCabin { Ship = ship1, CabinType = interior, Number = 101, Price = 450m },
            new ShipCabin { Ship = ship1, CabinType = oceanView, Number = 201, Price = 650m }
        ];

        ship2.Cabins =
        [
            new ShipCabin { Ship = ship2, CabinType = suite, Number = 1, Price = 1500m },
            new ShipCabin { Ship = ship2, CabinType = oceanView, Number = 202, Price = 900m }
        ];

        ship3.Cabins =
        [
            new ShipCabin { Ship = ship3, CabinType = interior, Number = 301, Price = 400m },
            new ShipCabin { Ship = ship3, CabinType = oceanView, Number = 302, Price = 600m }
        ];

        ship4.Cabins =
        [
            new ShipCabin { Ship = ship4, CabinType = interior,  Number = 401, Price = 520m },
            new ShipCabin { Ship = ship4, CabinType = oceanView, Number = 402, Price = 750m },
            new ShipCabin { Ship = ship4, CabinType = suite,     Number = 403, Price = 1800m }
        ];

        ship5.Cabins =
        [
            new ShipCabin { Ship = ship5, CabinType = interior,  Number = 501, Price = 380m },
            new ShipCabin { Ship = ship5, CabinType = oceanView, Number = 502, Price = 580m }
        ];

        var cruise1 = new Cruise
        {
            Name = "Caribbean Explorer",
            Code = "CRU001",
            Zone = "Caribe",
            Description = "7 noches recorriendo las mejores islas del Caribe",
            OriginPort = "Miami",
            Itinerary = "Miami - Bahamas - Isla A - Isla B - Miami",
            DurationInDays = 7,
            AdultsOnly = true,
            Featured = true
        };

        var cruise2 = new Cruise
        {
            Name = "Mediterranean Luxury",
            Code = "CRU002",
            Zone = "Mediterráneo",
            Description = "9 noches por el Mediterráneo con paradas en puertos históricos",
            OriginPort = "Barcelona",
            Itinerary = "Barcelona - Marsella - Génova - Roma - Nápoles - Barcelona",
            DurationInDays = 9,
            AdultsOnly = false,
            Featured = true
        };

        var cruise3 = new Cruise
        {
            Name = "Northern Adventure",
            Code = "CRU003",
            Zone = "Atlántico Norte",
            Description = "5 noches de exploración y actividades al aire libre",
            OriginPort = "Reykjavik",
            Itinerary = "Reykjavik - Islandia - Islas - Reykjavik",
            DurationInDays = 5,
            AdultsOnly = true,
            Featured = false
        };

        var cruise4 = new Cruise
        {
            Name = "Pacific Getaway",
            Code = "CRU004",
            Zone = "Pacífico",
            Description = "14 noches descubriendo paraísos del Pacífico sur, ideal para familias",
            OriginPort = "Los Ángeles",
            Itinerary = "Los Ángeles - Honolulu - Papeete - Bora Bora - Los Ángeles",
            DurationInDays = 14,
            AdultsOnly = false,
            Featured = false
        };

        var cruise5 = new Cruise
        {
            Name = "Baltic Discovery",
            Code = "CRU005",
            Zone = "Báltico",
            Description = "10 noches navegando por los fiordos y capitales del mar Báltico",
            OriginPort = "Copenhague",
            Itinerary = "Copenhague - Oslo - Helsinki - Tallin - Estocolmo - Copenhague",
            DurationInDays = 10,
            AdultsOnly = false,
            Featured = true
        };

        var now = DateTime.UtcNow;
        var cd1Start = now.AddMonths(1);
        var cd2Start = now.AddMonths(3);
        var cd3Start = now.AddMonths(2);
        var cd4Start = now.AddMonths(1).AddDays(10);
        var cd5Start = now.AddMonths(4);
        var cd6Start = now.AddMonths(2).AddDays(5);
        var cd7Start = now.AddMonths(5);
        var cd8Start = now.AddMonths(3);

        var cd1 = new CruiseDate { Cruise = cruise1, Ship = ship1, StartDate = cd1Start, PaymentStartDate = cd1Start.AddDays(-30), CheckInStartDate = cd1Start.AddDays(-15), PriceModifier = 1.00m };
        var cd2 = new CruiseDate { Cruise = cruise1, Ship = ship1, StartDate = cd2Start, PaymentStartDate = cd2Start.AddDays(-30), CheckInStartDate = cd2Start.AddDays(-15), PriceModifier = 1.10m };

        var cd3 = new CruiseDate { Cruise = cruise2, Ship = ship2, StartDate = cd3Start, PaymentStartDate = cd3Start.AddDays(-30), CheckInStartDate = cd3Start.AddDays(-15), PriceModifier = 1.25m };

        var cd4 = new CruiseDate { Cruise = cruise3, Ship = ship3, StartDate = cd4Start, PaymentStartDate = cd4Start.AddDays(-30), CheckInStartDate = cd4Start.AddDays(-15), PriceModifier = 0.95m };
        var cd5 = new CruiseDate { Cruise = cruise3, Ship = ship3, StartDate = cd5Start, PaymentStartDate = cd5Start.AddDays(-30), CheckInStartDate = cd5Start.AddDays(-15), PriceModifier = 1.05m };
        var cd6 = new CruiseDate { Cruise = cruise4, Ship = ship4, StartDate = cd6Start, PaymentStartDate = cd6Start.AddDays(-30), CheckInStartDate = cd6Start.AddDays(-15), PriceModifier = 1.15m };
        var cd7 = new CruiseDate { Cruise = cruise4, Ship = ship4, StartDate = cd7Start, PaymentStartDate = cd7Start.AddDays(-30), CheckInStartDate = cd7Start.AddDays(-15), PriceModifier = 1.00m };
        var cd8 = new CruiseDate { Cruise = cruise5, Ship = ship5, StartDate = cd8Start, PaymentStartDate = cd8Start.AddDays(-30), CheckInStartDate = cd8Start.AddDays(-15), PriceModifier = 1.20m };

        var cde1 = new CruiseDateExtra { CruiseDate = cd1, Extra = wifi, Price = 25m };
        var cde2 = new CruiseDateExtra { CruiseDate = cd1, Extra = drinks, Price = 120m };
        var cde3 = new CruiseDateExtra { CruiseDate = cd3, Extra = excursion, Price = 80m };
        var cde4 = new CruiseDateExtra { CruiseDate = cd6, Extra = wifi, Price = 25m };
        var cde5 = new CruiseDateExtra { CruiseDate = cd6, Extra = drinks, Price = 130m };
        var cde6 = new CruiseDateExtra { CruiseDate = cd8, Extra = excursion, Price = 95m };

        IEnumerable<object> toAdd =
        [
            interior, oceanView, suite,
            wifi, drinks, excursion,
            ship1, ship2, ship3, ship4, ship5,
            cruise1, cruise2, cruise3, cruise4, cruise5,
            cd1, cd2, cd3, cd4, cd5, cd6, cd7, cd8,
            cde1, cde2, cde3, cde4, cde5, cde6
        ];

        cruisesDbContext.AddRange(toAdd);
        cruisesDbContext.SaveChanges();
    }
}
