﻿namespace Core.Agents.Common;

/// <summary>
/// Identifiers of the agents that take part in the handoff orchestration.
/// These values are used as <c>ExecutorId</c> in workflow events and traces.
/// </summary>
internal static class AgentNames
{
    public const string Triage = "triage_agent";
    public const string Booking = "booking_agent";
    public const string PostSales = "post_sales_agent";
}
