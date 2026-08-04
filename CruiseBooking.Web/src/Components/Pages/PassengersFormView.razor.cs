using CruiseBooking.Integrations.Models;
using CruiseBooking.Services.Api.CheckIn;
using CruiseBooking.Vms;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CruiseBooking.Components.Pages;

/// <summary>
/// Muestra un formulario para agregar un nuevo pasajero a un camarote durante el check-in.
/// </summary>
public partial class PassengersFormView(NavigationManager navigationManager, ISnackbar snackbar)
{
    readonly NavigationManager _navigationManager = navigationManager;
    readonly ISnackbar _snackbar = snackbar;

    /// <summary>
    /// Obtiene o establece la puerta de enlace de check-in que maneja las operaciones de API.
    /// </summary>
    [Parameter, EditorRequired]
    public ICheckInGateway Gateway { get; set; } = null!;

    /// <summary>
    /// Obtiene o establece el identificador único del camarote para agregar un pasajero.
    /// </summary>
    [Parameter, EditorRequired]
    public int CabinId { get; set; }

    bool _isLoading = true;
    string _cabinType = string.Empty;
    string _cabinDescription = string.Empty;
    int _maxOccupancy;
    int _registeredCount;
    bool _submitting;

    readonly OccupantVm _passenger = new();
    MudForm? _form;
    readonly PassengersFormValidator passengerValidator = new();

    class PassengersFormValidator: AbstractValidator<OccupantVm>
    {
        public PassengersFormValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("El nombre es obligatorio.");

            RuleFor(p => p.Surname)
                .NotEmpty().WithMessage("Los apellidos son obligatorios.");

            RuleFor(p => p.BirthDate)
                .NotNull().WithMessage("La fecha de nacimiento es obligatoria.")
                .Must(d => d <= DateTime.Today)
                    .WithMessage("La fecha de nacimiento no puede ser futura.");

            RuleFor(p => p.Dni)
                .NotEmpty().WithMessage("El DNI es obligatorio.");

            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("El email es obligatorio.")
                .EmailAddress().WithMessage("El formato de email no es válido.");

            RuleFor(p => p.Phone)
                .NotEmpty().WithMessage("El teléfono de contacto es obligatorio.");

            RuleFor(p => p.Addres)
                .NotEmpty().WithMessage("La dirección es obligatoria.");
        }

        public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
        {
            var result = await ValidateAsync(ValidationContext<OccupantVm>.CreateWithOptions((OccupantVm)model, x => x.IncludeProperties(propertyName)));
            return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
        };
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadCabinAsync();
    }

    async Task LoadCabinAsync()
    {
        try
        {
            var result = await Gateway.Get();
            var cabin = result.IsSuccess
                ? result.Value!.Cabins.FirstOrDefault(c => c.BookingCabinId == CabinId)
                : null;

            if (cabin is null)
            {
                _snackbar.Add("No se pudo cargar el camarote. Por favor, inténtalo de nuevo.", MudBlazor.Severity.Warning);
                _navigationManager.NavigateTo(Gateway.CheckInUrl());
                return;
            }

            _cabinType = cabin.CabinType;
            _cabinDescription = cabin.CabinDescription;
            _maxOccupancy = cabin.Occupancy;
            _registeredCount = cabin.Passengers.Count;

            if (_registeredCount >= _maxOccupancy)
            {
                _snackbar.Add("Este camarote ya tiene registrados todos sus pasajeros.", MudBlazor.Severity.Info);
                _navigationManager.NavigateTo(Gateway.CheckInUrl());
                return;
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    async Task SubmitAsync()
    {
        if (_form is null)
            return;

        await _form.Validate();
        if (!_form.IsValid)
            return;

        _submitting = true;

        var request = new CheckInPassengerRequest
        {
            Name = _passenger.Name,
            Surname = _passenger.Surname,
            BirthDate = _passenger.BirthDate!.Value,
            Dni = _passenger.Dni,
            Email = _passenger.Email,
            Address = _passenger.Addres,
            Phone = _passenger.Phone
        };

        try
        {
            var result = await Gateway.AddPassenger(CabinId, request);
            _snackbar.Add(
                result.IsSuccess
                    ? "Pasajero añadido correctamente."
                    : result.ErrorMessage!,
                result.IsSuccess ? MudBlazor.Severity.Success : MudBlazor.Severity.Error);
        }
        finally
        {
            _submitting = false;
        }

        _navigationManager.NavigateTo(Gateway.CheckInUrl());
    }
}
