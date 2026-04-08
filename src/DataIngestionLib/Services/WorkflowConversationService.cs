// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using DataIngestionLib.Agents;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Services;





public interface IWorkflowConversationService
{
    Task<bool> InitializeAsync();


    Task<List<ChatMessage>> RunStreamingWorkflowAsync(string userMessage, Func<ChatMessage, CancellationToken, Task>? onMessageUpdate = null, CancellationToken cancellationToken = default);
}





public class WorkflowConversationService : IWorkflowConversationService
{
    private readonly IAgentFactory _agentFactory;
    private Workflow _workflow;
    private readonly ILogger<WorkflowConversationService> _logger;








    public WorkflowConversationService(IAgentFactory agentFactory, ILogger<WorkflowConversationService> logger)
    {
        _agentFactory = agentFactory;
        _logger = logger;
    }


    public Task<bool> InitializeAsync()
    {

        var instrt = PredefinedAgentTypeExtensions.GetDefaultInstructions(PredefinedAgentType.CodeAssistant);



        AIAgent ag1 = _agentFactory.BuildBasicAgent(_agentFactory.GetChatClient(AIModels.LLAMA1_B.ToString()), "Coder1", "", instrt);
        AIAgent ag2 = _agentFactory.BuildBasicAgent(_agentFactory.GetChatClient(AIModels.LLAMA1_B.ToString()), "Coder2", "", instrt);
        IEnumerable<AIAgent> agents = new[] { ag1, ag2 };
        // Build group chat with round-robin speaker selection
        // The manager factory receives the list of agents and returns a configured manager

        _workflow = this.buildWorkflow(agents);





        return Task.FromResult(true);
    }








    private Workflow buildWorkflow(IEnumerable<AIAgent> agents)
    {

        Workflow workflow = AgentWorkflowBuilder
                .CreateGroupChatBuilderWith(agents =>
                        new RoundRobinGroupChatManager(agents)
                        {
                            MaximumIterationCount = 3,

                        })
                .AddParticipants(agents)
                .Build();
        return workflow;

    }







    public async Task<List<ChatMessage>> RunStreamingWorkflowAsync(string userMessage, Func<ChatMessage, CancellationToken, Task>? onMessageUpdate = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);
        // Start the group chat
        List<ChatMessage> messages = new()
        {
                new(ChatRole.User, userMessage)
        };

        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(_workflow, messages);
        var unused = await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
        {
            if (evt is AgentResponseUpdateEvent update)
            {
                // Process streaming agent responses
                AgentResponse response = update.AsResponse();
                foreach (ChatMessage message in response.Messages)
                {
                    if (onMessageUpdate is not null)
                    {
                        await onMessageUpdate(message, cancellationToken).ConfigureAwait(false);
                    }

                    _logger.LogInformation($"[{update.ExecutorId}]: {message.Text}");
                }
            }
            else if (evt is WorkflowOutputEvent output)
            {
                // Workflow completed
                List<ChatMessage>? conversationHistory = output.As<List<ChatMessage>>();
                _logger.LogInformation("\n=== Final Conversation ===");
                foreach (ChatMessage message in conversationHistory)
                {
                    _logger.LogInformation($"{message.AuthorName}: {message.Text}");
                }

                return conversationHistory;
                break;
            }

        }

        return new List<ChatMessage>();
    }
















}
