﻿using Microsoft.Agents.AI;

namespace Core.Agents.Orchestration;

/// <summary>
/// Interface for building the agents used in a chat session
/// </summary>
internal interface IAgentFactory
{
    /// <summary>
    /// Builds the agents that serve the given chat session.
    /// </summary>
    /// <param name="sessionId">Identifier of the chat session. Scopes the booking draft in the cache.</param>
    /// <returns>The agents and tool instances bound to that session.</returns>
    AgentTeam CreateChatAgents(Guid sessionId);
}

/// <summary>
/// Agents and tool instances that serve a single chat session.
/// </summary>
/// <param name="Triage">Agent that starts the handoff workflows.</param>
/// <param name="Booking">Agent that searches the catalogue and handles the booking process</param>
/// <param name="PostSales">Agent that manages the bookings the user already has</param>
internal sealed record AgentTeam(
    AIAgent Triage,
    AIAgent Booking,
    AIAgent PostSales);
