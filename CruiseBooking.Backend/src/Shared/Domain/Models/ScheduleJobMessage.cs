using System.Text.Json;

namespace Shared.Domain.Models;

/// <summary>
/// Mensaje de pub/sub que solicita registrar un job de un solo disparo en el planificador de Dapr.
/// </summary>
/// <param name="JobName">
/// Nombre único con el que el job se registra en el planificador de Dapr.
/// Opcional en la deserialización para que un mensaje sin nombre se descarte (DROP)
/// en la validación en lugar de provocar un 400 y un bucle de reentregas.
/// </param>
/// <param name="DueTime">
/// Momento en el que debe dispararse el job. Si no se indica, se planifica para el momento actual.
/// </param>
/// <param name="Payload">
/// Carga útil opaca que Dapr entregará al handler cuando el job se dispare.
/// </param>
public record ScheduleJobRequest(string? JobName, DateTimeOffset? DueTime, JsonElement? Payload);
