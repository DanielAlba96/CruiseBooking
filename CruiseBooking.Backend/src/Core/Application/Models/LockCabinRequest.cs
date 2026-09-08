namespace Core.Application.Models;

/// <summary>Solicitud para bloquear un único camarote de una fecha de crucero.</summary>
public class LockCabinRequest
{
    /// <summary>Identificador de la fecha de crucero.</summary>
    public int CruiseDateId { get; set; }

    /// <summary>Número de pasajeros que ocuparán el camarote.</summary>
    public int Occupants { get; set; }
}
