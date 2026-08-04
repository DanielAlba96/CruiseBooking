using System;
using Shared.Domain.Entities;

namespace Core.Application.Models;

/// <summary>
/// Respuesta DTO con el resumen de reserva del usuario actual.
/// </summary>
public class GetCurrentUserBookingSummaryDtoResponse
{
    /// <summary>
    /// Identificador único de la reserva.
    /// </summary>
    /// <value>Número entero que identifica la reserva.</value>
    public int Id { get; set; }

    /// <summary>
    /// Indica si la reserva ha sido cancelada.
    /// </summary>
    /// <value><c>true</c> si la reserva está cancelada; en caso contrario, <c>false</c>.</value>
    public bool IsCanceled { get; set; }

    /// <summary>
    /// Estado actual de la reserva.
    /// </summary>
    /// <value>Estado de la reserva según la enumeración <see cref="BookingStatus"/>.</value>
    public BookingStatus Status { get; set; }

    /// <summary>
    /// Nombre de la reserva o crucero.
    /// </summary>
    /// <value>Cadena de texto con el nombre del crucero.</value>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción de la reserva o crucero.
    /// </summary>
    /// <value>Cadena de texto con la descripción del crucero.</value>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Destino del crucero.
    /// </summary>
    /// <value>Cadena de texto con el destino principal del crucero.</value>
    public string Destination { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de inicio del crucero.
    /// </summary>
    /// <value>Fecha y hora en UTC del comienzo del crucero.</value>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Fecha de fin del crucero.
    /// </summary>
    /// <value>Fecha y hora en UTC del final del crucero.</value>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Cantidad de pasajeros en la reserva.
    /// </summary>
    /// <value>Número entero representando el total de pasajeros.</value>
    public int PassengerCount { get; set; }
}
