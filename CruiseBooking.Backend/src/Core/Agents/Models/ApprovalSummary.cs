using Core.Agents.Common;
using System.Text.Json.Serialization;

namespace Core.Agents.Models;

/// <summary>
/// Resumen estructurado de una operación pendiente de aprobación por parte del usuario.
/// La jerarquía se serializa de forma polimórfica para que el cliente pueda deserializar
/// el tipo concreto y renderizarlo con sus propios componentes. Los importes viajan como
/// <see cref="decimal"/> y las fechas como <see cref="DateTime"/> en UTC: todo el formateo
/// es responsabilidad del cliente.
/// </summary>
/// <param name="Title">Título del resumen que se muestra al usuario.</param>
[JsonPolymorphic]
[JsonDerivedType(typeof(BookingConfirmationSummary), BookingToolNames.ConfirmBooking)]
[JsonDerivedType(typeof(PaymentSummary), PostSalesToolNames.PayBooking)]
[JsonDerivedType(typeof(CancellationSummary), PostSalesToolNames.CancelBooking)]
public abstract record ApprovalSummary(string Title);

/// <summary>
/// Línea de camarote dentro de un resumen de aprobación.
/// </summary>
/// <param name="Name">Nombre o tipo del camarote.</param>
/// <param name="Occupants">Cantidad de pasajeros que ocuparán el camarote.</param>
/// <param name="Price">Precio del camarote.</param>
public sealed record SummaryCabinLine(string Name, int Occupants, decimal Price);

/// <summary>
/// Línea de extra dentro de un resumen de aprobación.
/// </summary>
/// <param name="Name">Nombre descriptivo del extra.</param>
/// <param name="Price">Precio del extra.</param>
public sealed record SummaryExtraLine(string Name, decimal Price);

/// <summary>
/// Cabecera con los datos de una reserva ya existente.
/// </summary>
/// <param name="BookingId">Identificador de la reserva.</param>
/// <param name="CruiseName">Nombre del crucero.</param>
/// <param name="Destination">Destino del crucero.</param>
/// <param name="ShipName">Nombre del barco.</param>
/// <param name="StartDate">Fecha de salida en UTC.</param>
/// <param name="EndDate">Fecha de regreso en UTC.</param>
public sealed record SummaryBookingHeader(
    int BookingId,
    string CruiseName,
    string Destination,
    string ShipName,
    DateTime StartDate,
    DateTime EndDate);

/// <summary>
/// Método de pago elegido para un cobro pendiente de aprobación.
/// </summary>
/// <param name="Brand">Marca de la tarjeta.</param>
/// <param name="Last4">Últimos cuatro dígitos de la tarjeta.</param>
/// <param name="ExpMonth">Mes de caducidad.</param>
/// <param name="ExpYear">Año de caducidad.</param>
public sealed record SummaryPaymentMethod(string Brand, string Last4, int ExpMonth, int ExpYear);

/// <summary>
/// Resumen de la reserva en construcción que el usuario debe confirmar.
/// </summary>
/// <param name="Title">Título del resumen.</param>
/// <param name="CruiseName">Nombre del crucero.</param>
/// <param name="ShipName">Nombre del barco.</param>
/// <param name="DepartureDate">Fecha de salida en UTC.</param>
/// <param name="Cabins">Camarotes seleccionados.</param>
/// <param name="Extras">Extras seleccionados.</param>
/// <param name="Total">Importe total de la reserva.</param>
public sealed record BookingConfirmationSummary(
    string Title,
    string CruiseName,
    string ShipName,
    DateTime DepartureDate,
    IReadOnlyList<SummaryCabinLine> Cabins,
    IReadOnlyList<SummaryExtraLine> Extras,
    decimal Total) : ApprovalSummary(Title);

/// <summary>
/// Resumen del cobro manual de una reserva existente.
/// </summary>
/// <param name="Title">Título del resumen.</param>
/// <param name="Booking">Cabecera de la reserva a cobrar.</param>
/// <param name="Cabins">Camarotes de la reserva.</param>
/// <param name="Extras">Extras de la reserva.</param>
/// <param name="PaymentMethod">Método de pago elegido.</param>
/// <param name="Amount">Importe a cobrar.</param>
public sealed record PaymentSummary(
    string Title,
    SummaryBookingHeader Booking,
    IReadOnlyList<SummaryCabinLine> Cabins,
    IReadOnlyList<SummaryExtraLine> Extras,
    SummaryPaymentMethod PaymentMethod,
    decimal Amount) : ApprovalSummary(Title);

/// <summary>
/// Resumen de la cancelación de una reserva existente.
/// </summary>
/// <param name="Title">Título del resumen.</param>
/// <param name="Booking">Cabecera de la reserva a cancelar.</param>
/// <param name="Cabins">Camarotes de la reserva.</param>
/// <param name="Extras">Extras de la reserva.</param>
/// <param name="AlreadyCharged">Indica si la reserva ya está cobrada.</param>
/// <param name="RefundAmount">Importe a reembolsar; cero si la reserva no está cobrada.</param>
public sealed record CancellationSummary(
    string Title,
    SummaryBookingHeader Booking,
    IReadOnlyList<SummaryCabinLine> Cabins,
    IReadOnlyList<SummaryExtraLine> Extras,
    bool AlreadyCharged,
    decimal RefundAmount) : ApprovalSummary(Title);
