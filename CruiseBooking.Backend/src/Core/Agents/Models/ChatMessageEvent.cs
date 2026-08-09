namespace Core.Agents.Models;

public abstract record ChatStreamEvent;

public sealed record ChatStreamEventToken(string Text) : ChatStreamEvent;

public sealed record ChatStreamEventApproval(string CallId, string Summary) : ChatStreamEvent;

public sealed record ChatStreamEventError(string Reason) : ChatStreamEvent;
