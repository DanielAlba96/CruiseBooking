using CruiseBooking.Services.Api;
using CruiseBooking.Services.Api.CheckIn;
using Microsoft.AspNetCore.Components;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Inicia el proceso de check-in usando un identificador de reserva autenticado.
/// </summary>
public partial class CheckInBooking(IBookingApiService api)
{
    readonly IBookingApiService _api = api;

    /// <summary>
    /// Obtiene o establece el identificador único de la reserva.
    /// </summary>
    [Parameter]
    public int BookingId { get; set; }

    ICheckInGateway _gateway = null!;

    protected override void OnParametersSet()
    {
        _gateway = new BookingCheckInGateway(_api, BookingId);
    }
}
