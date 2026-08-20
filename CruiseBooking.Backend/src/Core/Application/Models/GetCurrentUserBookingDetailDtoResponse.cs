using System;
using System.Collections.Generic;
using Shared.Domain.Entities;

namespace Core.Application.Models;

/// <summary>
/// Respuesta DTO con los detalles de una reserva actual del usuario.
/// </summary>
public class GetCurrentUserBookingDetailDtoResponse
{
    /// <summary>
    /// Identificador único de la reserva.
    /// </summary>
    /// <value>Número entero que representa el identificador de la reserva.</value>
    public int Id { get; set; }

    /// <summary>
    /// Indica si la reserva ha sido cancelada.
    /// </summary>
    /// <value>Verdadero si la reserva está cancelada; en caso contrario, falso.</value>
    public bool IsCanceled { get; set; }

    /// <summary>
    /// Estado actual de la reserva.
    /// </summary>
    /// <value>Valor de enumeración que representa el estado de la reserva.</value>
    public BookingStatus Status { get; set; }

    /// <summary>
    /// Nombre de la reserva o crucero.
    /// </summary>
    /// <value>Cadena de texto que contiene el nombre.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Código único de la reserva.
    /// </summary>
    /// <value>Cadena de texto que contiene el código de identificación.</value>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del crucero.
    /// </summary>
    /// <value>Cadena de texto con la descripción.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Destino del crucero.
    /// </summary>
    /// <value>Cadena de texto que identifica el destino.</value>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Puerto de origen del crucero.
    /// </summary>
    /// <value>Cadena de texto con el nombre del puerto de salida.</value>
    public string OriginPort { get; set; } = string.Empty;

    /// <summary>
    /// Itinerario del crucero.
    /// </summary>
    /// <value>Cadena de texto que describe el itinerario.</value>
    public string Itinerary { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de inicio del crucero.
    /// </summary>
    /// <value>Fecha y hora de inicio en formato UTC.</value>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Fecha de finalización del crucero.
    /// </summary>
    /// <value>Fecha y hora de finalización en formato UTC.</value>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Fecha de inicio del período de pago.
    /// </summary>
    /// <value>Fecha y hora en formato UTC.</value>
    public DateTime PaymentStartDate { get; set; }

    /// <summary>
    /// Fecha de inicio del check-in.
    /// </summary>
    /// <value>Fecha y hora en formato UTC.</value>
    public DateTime CheckInStartDate { get; set; }

    /// <summary>
    /// Duración del crucero en días.
    /// </summary>
    /// <value>Número entero que representa la cantidad de días.</value>
    public int DurationInDays { get; set; }

    /// <summary>
    /// Nombre del barco.
    /// </summary>
    /// <value>Cadena de texto con el nombre del barco.</value>
    public string ShipName { get; set; } = string.Empty;

    /// <summary>
    /// Compañía naviera.
    /// </summary>
    /// <value>Cadena de texto con el nombre de la compañía.</value>
    public string ShipCompany { get; set; } = string.Empty;

    /// <summary>
    /// Cantidad de pasajeros.
    /// </summary>
    /// <value>Número entero que representa la cantidad de personas.</value>
    public int PassengerCount { get; set; }

    /// <summary>
    /// Fecha de creación de la reserva.
    /// </summary>
    /// <value>Fecha y hora en formato UTC.</value>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha en la que se realizó el cobro.
    /// </summary>
    /// <value>Fecha y hora en formato UTC, o nulo si no se ha cobrado aún.</value>
    public DateTime? ChargedAt { get; set; }

    /// <summary>
    /// Fecha en la que se realizó el reembolso.
    /// </summary>
    /// <value>Fecha y hora en formato UTC, o nulo si no se ha reembolsado.</value>
    public DateTime? RefundedAt { get; set; }

    /// <summary>
    /// Lista de camarotes asociados a la reserva.
    /// </summary>
    /// <value>Colección de detalles de camarotes.</value>
    public List<GetCurrentUserBookingCabinDtoResponse> Cabins { get; set; } = new();

    /// <summary>
    /// Lista de servicios adicionales asociados a la reserva.
    /// </summary>
    /// <value>Colección de detalles de extras.</value>
    public List<GetCurrentUserBookingExtraDtoResponse> Extras { get; set; } = new();
}
