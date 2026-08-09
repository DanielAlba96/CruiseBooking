using Microsoft.Agents.AI.Workflows;

namespace Core.Agents.Orchestration;

internal interface IWorkflowFactory
{
    Workflow CreateChatWorkflow(Guid sessionId);
}