using Core.Agents.Common;
using Microsoft.Agents.AI.Compaction;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

#pragma warning disable MAAI001 // El framework de compaction esta actualmente en fase experimental

namespace Core.Agents.Chat.Compaction;

/// <summary>
/// Estrategia de compactación que reduce las busquedas al crucero seleccionado
/// y elimina ejecuciones de herramientas pertenecientes a reservas anteriores
/// </summary>
internal sealed class BookingContextCompactionStrategy() : CompactionStrategy(CompactionTriggers.HasToolCalls())
{
    /// <inheritdoc />
    protected override ValueTask<bool> CompactCoreAsync(
        CompactionMessageIndex index,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(index);

        var changed = FilterSearches(index);

        if (changed)
            logger.LogDebug("Descartados del contexto los resultados de herramientas obsoletos");

        return ValueTask.FromResult(changed);
    }

    private static bool FilterSearches(CompactionMessageIndex index)
    {
        var marker = FindLastCall(index, BookingToolNames.AddCabin);
        if (marker is null)
            return false;

        var cruiseDateId = GetCruiseDateId(marker.Value.Call);
        if (cruiseDateId is null)
            return false;

        var changed = false;

        for (var position = 0; position < marker.Value.Position; position++)
        {
            var group = index.Groups[position];
            if (group.Kind != CompactionGroupKind.ToolCall)
                continue;

            var calls = group.Messages.SelectMany(x => x.Contents).OfType<FunctionCallContent>();

            var searches = calls.Where(x => x.Name == BookingToolNames.SearchCruises);
            foreach (var callId in searches.Select(x => x.CallId))
            {
                var result = group.Messages
                    .SelectMany(x => x.Contents)
                    .OfType<FunctionResultContent>()
                    .FirstOrDefault(x => x.CallId == callId);

                if (result is null)
                    continue;

                var reduced = FilterCruises(ReadJson(result), cruiseDateId.Value);
                if (reduced is null)
                    RemovePair(group, callId);
                else
                    result.Result = reduced;

                changed = true;
            }

            var obsoleteCalls = calls.Where(x => BookingToolNames.DraftScoped.Contains(x.Name));
            foreach (var callId in obsoleteCalls.Select(x => x.CallId))
            {
                RemovePair(group, callId);
                changed = true;
            }
        }

        return changed;
    }

    private static (int Position, FunctionCallContent Call)? FindLastCall(CompactionMessageIndex index, string toolName)
    {
        for (var position = index.Groups.Count - 1; position >= 0; position--)
        {
            var group = index.Groups[position];
            if (group.Kind != CompactionGroupKind.ToolCall)
                continue;

            var calls = group.Messages.SelectMany(x => x.Contents).OfType<FunctionCallContent>();
            foreach (var call in calls.Where(x => x.Name == toolName).Reverse())
            {
                var result = group.Messages
                    .SelectMany(x => x.Contents)
                    .OfType<FunctionResultContent>()
                    .FirstOrDefault(x => x.CallId == call.CallId);

                if (result is not null && IsSuccessful(result))
                    return (position, call);
            }
        }

        return null;
    }

    private static void RemovePair(CompactionMessageGroup group, string callId)
    {
        foreach (var messages in group.Messages.Select(x => x.Contents))
        {
            for (var i = messages.Count - 1; i >= 0; i--)
            {
                if (messages[i] is FunctionCallContent call && call.CallId == callId)
                    messages.RemoveAt(i);

                else if (messages[i] is FunctionResultContent result && result.CallId == callId)
                    messages.RemoveAt(i);
            }
        }
    }

    private static string? ReadJson(FunctionResultContent result) => result.Result switch
    {
        null => null,
        string text => text,
        JsonElement element => element.ValueKind == JsonValueKind.String ? element.GetString() : element.GetRawText(),
        var other => other.ToString()
    };

    private static bool IsSuccessful(FunctionResultContent result)
    {
        try
        {
            return JsonNode.Parse(ReadJson(result) ?? string.Empty)?["ok"]?.GetValue<bool>() ?? false;
        }
        catch (Exception e) when (e is JsonException or FormatException or InvalidOperationException)
        {
            return false;
        }
    }

    private static int? GetCruiseDateId(FunctionCallContent call)
    {
        if (call.Arguments is null || !call.Arguments.TryGetValue("cruiseDateId", out var value))
            return null;

        return value switch
        {
            int number => number,
            JsonElement { ValueKind: JsonValueKind.Number } element when element.TryGetInt32(out var number) => number,
            _ => null
        };
    }

    private static string? FilterCruises(string? json, int cruiseDateId)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var departureDateId = cruiseDateId.ToString(CultureInfo.InvariantCulture);
            var cruises = JsonNode.Parse(json)?["cruises"]?.AsArray();
            var cruise = cruises?.FirstOrDefault(x => x?["DepartureDates"]?[departureDateId] is not null);
            if (cruise is null)
                return null;

            return new JsonObject
            {
                ["ok"] = true,
                ["cruises"] = new JsonArray(cruise.DeepClone())
            }.ToJsonString();
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException)
        {
            return null;
        }
    }
}

#pragma warning restore MAAI001 // El framework de compaction esta actualmente en fase experimental
