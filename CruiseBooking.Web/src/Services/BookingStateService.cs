using CruiseBooking.Services.Dtos;
using CruiseBooking.Vms;

namespace CruiseBooking.Services;

/// <summary>
/// Gestiona el estado de una reserva a través de varios pasos del asistente dentro de un circuito Blazor.
/// </summary>
public class BookingStateService
{
    /// <summary>
    /// Obtiene la información del crucero seleccionado.
    /// </summary>
    public CruiseSelectionVm? Cruise { get; private set; }

    /// <summary>
    /// Obtiene los camarotes seleccionados para esta reserva.
    /// </summary>
    public List<CabinSelectionVm> SelectedCabins { get; private set; } = [];

    /// <summary>
    /// Obtiene los extras seleccionados para esta reserva.
    /// </summary>
    public List<ExtraSelectionVm> SelectedExtras { get; private set; } = [];

    /// <summary>
    /// Guarda la selección de crucero y extras en el estado actual de la reserva.
    /// </summary>
    /// <param name="cruise">La información del crucero seleccionado.</param>
    /// <param name="selectedExtras">La lista de extras seleccionados.</param>
    public void SaveBooking(CruiseSelectionVm? cruise, List<ExtraSelectionVm> selectedExtras)
    {
        Cruise = cruise;
        SelectedExtras = selectedExtras;
    }

    /// <summary>
    /// Actualiza los camarotes seleccionados para esta reserva.
    /// </summary>
    /// <param name="selectedCabins">La lista de camarotes seleccionados.</param>
    public void UpdateCabins(List<CabinSelectionVm> selectedCabins) => SelectedCabins = selectedCabins;

    /// <summary>
    /// Genera un DTO de reserva a partir del estado actual.
    /// </summary>
    /// <param name="paymentMethodId">El identificador del método de pago opcional a incluir en la reserva.</param>
    /// <returns>Un DTO de reserva que representa el estado actual.</returns>
    public BookingDto GenerateBookingDto(string? paymentMethodId = null)
    {
        return new BookingDto
        {
            CruiseDateId = Cruise!.CruiseDateId,
            SelectedCabins = [.. SelectedCabins.Select(c => new BookingCabinDto(c.Id, c.CurrentOccupancy, c.CurrentPrice))],
            SelectedExtras = [.. SelectedExtras.Select(x => x.Id)],
            PaymentMethodId = paymentMethodId
        };
    }

    /// <summary>
    /// Borra todo el estado de la reserva de este servicio.
    /// </summary>
    public void Clear()
    {
        Cruise = null;
        SelectedCabins = [];
        SelectedExtras = [];
    }
}