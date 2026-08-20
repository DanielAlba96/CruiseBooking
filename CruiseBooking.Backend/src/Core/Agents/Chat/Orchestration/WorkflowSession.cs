using Microsoft.Extensions.AI;
namespace Core.Agents.Chat.Orchestration;

internal sealed record WorkflowSession(Guid SessionId, int UserId, List<ChatMessage> Messages);
