using Microsoft.Agents.AI.Workflows;

namespace Core.Agents.Chat.Orchestration;

internal class WorkflowFactory(IAgentFactory agentFactory) : IWorkflowFactory
{
    private readonly IAgentFactory _agentFactory = agentFactory;

    public Workflow CreateChatWorkflow(Guid sessionId)
    {
        var agents = _agentFactory.CreateChatAgents(sessionId);

        return AgentWorkflowBuilder.CreateHandoffBuilderWith(agents.Triage)
            .WithHandoffs(agents.Triage, [agents.Booking, agents.PostSales])
            .WithHandoffs([agents.Booking, agents.PostSales], agents.Triage)
            .Build();
    }
}
