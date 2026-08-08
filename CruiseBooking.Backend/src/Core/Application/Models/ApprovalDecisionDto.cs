namespace Core.Application.Models;

/// <summary>
/// Modelo de transferencia de datos con la decisión del usuario sobre una herramienta que requiere aprobación.
/// </summary>
public class ApprovalDecisionDto
{
    /// <summary>
    /// Obtiene o establece un valor que indica si el usuario aprobó la ejecución de la herramienta.
    /// </summary>
    /// <value>
    /// <c>true</c> si el usuario confirmó la operación; <c>false</c> si la rechazó. Por defecto es <c>false</c>.
    /// </value>
    public bool Approved { get; set; }
}
