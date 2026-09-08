using Microsoft.Agents.AI.Workflows;

namespace Core.Agents.Chat.Orchestration;

internal interface IWorkflowFactory
{
    Workflow CreateChatWorkflow(Guid sessionId);
}