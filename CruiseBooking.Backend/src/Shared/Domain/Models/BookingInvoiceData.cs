namespace Shared.Domain.Models;

/// <summary>
/// Proyección de una reserva con los campos necesarios para generar y enviar la factura.
/// </summary>
/// <param name="Id">Identificador de la reserva.</param>
/// <param name="IsCanceled">Indica si la reserva está cancelada.</param>
/// <param name="ChargedAt">Fecha de pago.</param>
/// <param name="ChargeAmount">Importe total cobrado, impuestos incluidos.</param>
/// <param name="UserName">Nombre del usuario de la reserva.</param>
/// <param name="UserEmail">Email del usuario de la reserva.</param>
/// <param name="CruiseName">Nombre del crucero.</param>
/// <param name="CruiseCode">Código del crucero.</param>
/// <param name="OriginPort">Puerto de origen del crucero.</param>
/// <param name="DurationInDays">Duración del crucero en días.</param>
/// <param name="Itinerary">Ruta del crucero.</param>
/// <param name="StartDate">Fecha de salida del crucero.</param>
/// <param name="CheckInStartDate">Fecha límite de pago manual / inicio del check-in.</param>
/// <param name="ShipName">Nombre del barco.</param>
/// <param name="ShipCompany">Compañía del barco.</param>
/// <param name="Cabins">Líneas de camarotes de la factura.</param>
/// <param name="Extras">Líneas de extras de la factura.</param>
public sealed record BookingInvoiceData(
    int Id,
    bool IsCanceled,
    DateTime ChargedAt,
    decimal ChargeAmount,
    string UserName,
    string UserEmail,
    string CruiseName,
    string CruiseCode,
    string OriginPort,
    int DurationInDays,
    string Itinerary,
    DateTime StartDate,
    DateTime CheckInStartDate,
    string ShipName,
    string ShipCompany,
    List<InvoiceCabinLine> Cabins,
    List<InvoiceExtraLine> Extras);

/// <summary>Línea de camarote de la factura.</summary>
/// <param name="TypeName">Nombre del tipo de camarote.</param>
/// <param name="TypeDescription">Descripción del tipo de camarote.</param>
/// <param name="Occupants">Número de pasajeros del camarote.</param>
/// <param name="Price">Precio del camarote.</param>
public sealed record InvoiceCabinLine(
    string TypeName,
    string TypeDescription,
    int Occupants,
    decimal Price);

/// <summary>Línea de extra de la factura.</summary>
/// <param name="Name">Nombre del extra.</param>
/// <param name="Description">Descripción del extra.</param>
/// <param name="Price">Precio del extra.</param>
public sealed record InvoiceExtraLine(
    string Name,
    string Description,
    decimal Price);
