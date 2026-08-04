namespace Shared.Domain.Entities;

/// <summary>Estado del ciclo de vida de una reserva.</summary>
public enum BookingStatus
{
    /// <summary>La reserva se acaba de crear; el cobro automático inicial aún no se ha intentado.</summary>
    Created = 1,

    /// <summary>El cobro automático ha fallado; el usuario puede completar el pago de forma manual.</summary>
    PendingPayment = 2,

    /// <summary>La reserva está pagada pero los datos de los pasajeros aún no se han introducido.</summary>
    PendingCheckIn = 3,

    /// <summary>El check-in online se ha completado con los datos de todos los pasajeros.</summary>
    Complete = 4,

    /// <summary>La reserva se ha cancelado</summary>

    Canceled = 5
}
