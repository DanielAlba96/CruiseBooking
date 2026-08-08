using System.Text.Json.Serialization;

namespace CruiseBooking.Integrations.Models;

/// <summary>
/// Representa la decisión del usuario sobre una operación del asistente que requiere aprobación.
/// </summary>
/// <param name="Approved"><c>true</c> si el usuario confirma la operación; <c>false</c> si la rechaza.</param>
public record ApprovalDecisionRequest(
    [property: JsonPropertyName("approved")] bool Approved);
