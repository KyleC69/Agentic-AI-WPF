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





using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

using AgentAILib.Agents;
using AgentAILib.Models;

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Newtonsoft.Json;

using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;




namespace AgentAILib.Services;





public interface IWorkflowConversationService
{
    event EventHandler<bool>? BusyStateChanged;


    Task<bool> InitializeAsync();


    Task<string?> ExecuteWorkflow(string UserTask);


    Task<string?> ExecuteWorkflowAsync(Workflow workflow, string input);
}





/// <summary>
/// This sample demonstrates an iterative refinement workflow between Writer and Critic agents.
///
/// The workflow implements a content creation and review loop that:
/// 1. Writer creates initial content based on the user's request
/// 2. Critic reviews the content and provides feedback using structured output
/// 3. If approved: Summary executor presents the final content
/// 4. If rejected: Writer revises based on feedback (loops back)
/// 5. Continues until approval or max iterations (3) is reached
///
/// This pattern is useful when you need:
/// - Iterative content improvement through feedback loops
/// - Quality gates with reviewer approval
/// - Maximum iteration limits to prevent infinite loops
/// - Conditional workflow routing based on agent decisions
/// - Structured output for reliable decision-making
///
/// Key Learning: Workflows can implement loops with conditional edges, shared state,
/// and structured output for robust agent decision-making.
/// </summary>
/// <remarks>
/// - An IChatClient of your choice is required to run this sample (e.g., AzureOpenAIChatClient, OpenAIChatClient, Local).
/// </remarks>
public partial class WorkflowConversationService : IWorkflowConversationService
{
    private readonly IAgentFactory _agentFactory;
    private static ILogger<WorkflowConversationService> _logger = NullLogger<WorkflowConversationService>.Instance;
    public const int MaxIterations = 5;

    private Workflow _workflow = default!;
    private readonly string _sessionId = Guid.NewGuid().ToString("D");
    private bool _initialized;

    public event EventHandler<bool>? BusyStateChanged;







    public WorkflowConversationService(IAgentFactory agentFactory, ILogger<WorkflowConversationService> logger)
    {
        _agentFactory = agentFactory;
        _logger = logger;
    }








    public Task<bool> InitializeAsync()
    {
        if (_initialized)
        {
            return Task.FromResult(true);
        }

        IChatClient chatClient = _agentFactory.GetChatClient(AIModels.Default);

        // Create executors for content creation and review
        WriterExecutor writer = new(chatClient);
        CriticExecutor critic = new(chatClient);
        SummaryExecutor summary = new(chatClient);

        // Build the workflow with conditional routing based on critic's decision
        WorkflowBuilder workflowBuilder = new WorkflowBuilder(writer).AddEdge(writer, critic).AddSwitch(critic, sw => sw.AddCase<CriticDecision>(cd => cd?.Approved == true, summary).AddCase<CriticDecision>(cd => cd?.Approved == false, writer)).WithOutputFrom(summary);

        _workflow = workflowBuilder.Build();




        // Create a persistent session for multi‑turn collaboration
        _initialized = true;
        return Task.FromResult(true);
    }








    public async Task<string?> ExecuteWorkflow(string UserTask)
    {
        if (!_initialized)
        {
            _logger.LogWarning("WorkflowConversationService not initialized. Call InitializeAsync() first.");
            return "Workflow Conversation Service not initialized.";
        }

        BusyStateChanged?.Invoke(this, true);
        try
        {
            return await this.ExecuteWorkflowAsync(_workflow, UserTask);
        }
        finally
        {
            BusyStateChanged?.Invoke(this, false);
        }
    }








    public async Task<string?> ExecuteWorkflowAsync(Workflow workflow, string input)
    {
        // Execute in streaming mode to see real-time progress
        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, input);

        // Watch the workflow events
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            switch (evt)
            {
                case AgentResponseUpdateEvent agentUpdate:
                    // Stream agent output in real-time
                    if (!string.IsNullOrEmpty(agentUpdate.Update.Text))
                    {
                        _logger.LogInformation(agentUpdate.Update.Text);
                    }

                    break;

                case WorkflowOutputEvent output:
                    return output.Data?.ToString();
            }
        }

        return null;
    }








    // ====================================
    // Shared State for Iteration Tracking
    // ====================================





    /// <summary>
    /// Tracks the current iteration and conversation history across workflow executions.
    /// </summary>
    internal sealed class FlowState
    {
        public int Iteration { get; set; } = 1;
        public List<ChatMessage> History { get; } = [];
    }





    /// <summary>
    /// Constants for accessing the shared flow state in workflow context.
    /// </summary>
    internal static class FlowStateShared
    {
        public const string Scope = "FlowStateScope";
        public const string Key = "singleton";
    }





    /// <summary>
    /// Helper methods for reading and writing shared flow state.
    /// </summary>
    internal static class FlowStateHelpers
    {
        public static async Task<FlowState> ReadFlowStateAsync(IWorkflowContext context)
        {
            FlowState? state = await context.ReadStateAsync<FlowState>(FlowStateShared.Key, scopeName: FlowStateShared.Scope);
            return state ?? new FlowState();
        }








        public static ValueTask SaveFlowStateAsync(IWorkflowContext context, FlowState state)
        {
            return context.QueueStateUpdateAsync(FlowStateShared.Key, state, scopeName: FlowStateShared.Scope);
        }
    }





    // ====================================
    // Data Transfer Objects
    // ====================================





    /// <summary>
    /// Structured output schema for the Critic's decision.
    /// Uses JsonPropertyName and Description attributes for OpenAI's JSON schema.
    /// </summary>
    [Description("Critic's review decision including approval status and feedback")]
    [JsonArray]
    internal sealed class CriticDecision
    {
        [JsonPropertyName("approved")]
        [Description("Whether the content is approved (true) or needs revision (false)")]
        public bool Approved { get; set; }

        [JsonPropertyName("feedback")]
        [Description("Specific feedback for improvements if not approved, empty if approved")]
        public string Feedback { get; set; } = "";

        // Non-JSON properties for workflow use
        [System.Text.Json.Serialization.JsonIgnore] public string Content { get; set; } = "";

        [System.Text.Json.Serialization.JsonIgnore] public int Iteration { get; set; }
    }





    // ====================================
    // Custom Executors
    // ====================================





    /// <summary>
    /// Executor that creates or revises content based on user requests or critic feedback.
    /// This executor demonstrates multiple message handlers for different input types.
    /// </summary>
    internal sealed partial class WriterExecutor : Executor
    {
        private readonly AIAgent _agent;








        public WriterExecutor(IChatClient chatClient) : base("Writer")
        {
            _agent = new ChatClientAgent(chatClient, name: "Writer", instructions: """
                                                                                       You are a senior software engineer acting as the Coder.
                                                                                       Your job is to produce clean, correct, maintainable code that follows best practices.
                                                                                       When given feedback from the Lead engineer, revise the code carefully while preserving intent.
                                                                                       Avoid unnecessary abstractions, avoid over‑engineering, and ensure clarity and correctness.
                                                                                   """);

        }








        /// <summary>
        /// Handles the initial writing request from the user.
        /// </summary>
        [MessageHandler]
        public async ValueTask<ChatMessage> HandleInitialRequestAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            return await this.HandleAsyncCoreAsync(new ChatMessage(ChatRole.User, message), context, cancellationToken);
        }








        /// <summary>
        /// Handles revision requests from the critic with feedback.
        /// </summary>
        [MessageHandler]
        public async ValueTask<ChatMessage> HandleRevisionRequestAsync(CriticDecision decision, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            var prompt = "Revise the following content based on this feedback:\n\n" + $"Feedback: {decision.Feedback}\n\n" + $"Original Content:\n{decision.Content}";

            return await this.HandleAsyncCoreAsync(new ChatMessage(ChatRole.User, prompt), context, cancellationToken);
        }








        /// <summary>
        /// Core implementation for generating content (initial or revised).
        /// </summary>
        private async Task<ChatMessage> HandleAsyncCoreAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken)
        {
            FlowState state = await FlowStateHelpers.ReadFlowStateAsync(context);

            _logger.LogInformation($"\n=== Writer (Iteration {state.Iteration}) ===\n");

            StringBuilder sb = new();
            await foreach (AgentResponseUpdate update in _agent.RunStreamingAsync(message, cancellationToken: cancellationToken))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    _ = sb.Append(update.Text);
                    Console.Write(update.Text);
                }
            }

            _logger.LogInformation("\n");

            var text = sb.ToString();
            state.History.Add(new ChatMessage(ChatRole.Assistant, text));
            await FlowStateHelpers.SaveFlowStateAsync(context, state);

            return new ChatMessage(ChatRole.User, text);
        }
    }





    /// <summary>
    /// Executor that reviews content and decides whether to approve or request revisions.
    /// Uses structured output with streaming for reliable decision-making.
    /// </summary>
    internal sealed class CriticExecutor : Executor<ChatMessage, CriticDecision>
    {
        private readonly AIAgent _agent;








        public CriticExecutor(IChatClient chatClient) : base("Critic")
        {
            _agent = new ChatClientAgent(chatClient, new ChatClientAgentOptions
            {
                Name = "Critic",
                ChatOptions = new()
                {
                    Instructions = """
                                           You are the Lead Engineer performing code review.
                                             Evaluate correctness, maintainability, clarity, and adherence to best engineering practices.
                                             Provide structured output:
                                               - approved: true if the code is production‑ready
                                               - feedback: specific engineering improvements needed (empty if approved)
                                             Only approve when the code is high‑quality and meets the requirements.
                                             Response must be in strict JSON format matching the schema:
                                             {
                                               "approved": boolean,
                                               "feedback": string
                                             }
                                             Do *NOT* wrap in code fences
                                           """




                }
            });
        }








        public override async ValueTask<CriticDecision> HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            FlowState state = await FlowStateHelpers.ReadFlowStateAsync(context);

            _logger.LogInformation($"=== Critic (Iteration {state.Iteration}) ===\n");

            // Must use non streaming method to use type argument for Ollama Clients. They do not seem to support the ResponseFormat option.
            AgentResponse<CriticDecision> result = await _agent.RunAsync<CriticDecision>(message, cancellationToken: cancellationToken);

            var obj = ExtractBracketSpan(result.Text, '{', '}');

            // Deserialize the CriticDecision from the collected text
            CriticDecision decision = JsonSerializer.Deserialize<CriticDecision>(obj) ?? throw new JsonException("Failed to deserialize CriticDecision from streamed response text.");

            // Safety: approve if max iterations reached
            if (!decision.Approved && state.Iteration >= MaxIterations)
            {
                _logger.LogInformation($"⚠️ Max iterations ({MaxIterations}) reached - auto-approving");
                decision.Approved = true;
                decision.Feedback = "";
            }

            // Increment iteration ONLY if rejecting (will loop back to Writer)
            if (!decision.Approved)
            {
                state.Iteration++;
            }

            // Store the decision in history
            state.History.Add(new ChatMessage(ChatRole.Assistant, $"[Decision: {(decision.Approved ? "Approved" : "Needs Revision")}] {decision.Feedback}"));
            await FlowStateHelpers.SaveFlowStateAsync(context, state);

            // Populate workflow-specific fields
            decision.Content = message.Text ?? "";
            decision.Iteration = state.Iteration;

            return decision;
        }
    }


    public static string ExtractBracketSpan(string input, char openBracket, char closeBracket)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        var start = input.IndexOf(openBracket);
        var end = input.LastIndexOf(closeBracket);

        return start < 0 || end < 0 || end < start ? string.Empty : input.Substring(start, end - start + 1);
    }



    /// <summary>
    /// Executor that presents the final approved content to the user.
    /// </summary>
    internal sealed class SummaryExecutor : Executor<CriticDecision, ChatMessage>
    {
        private readonly AIAgent _agent;








        public SummaryExecutor(IChatClient chatClient) : base("Summary")
        {
            _agent = new ChatClientAgent(chatClient, name: "Summary", instructions: """
                                                                                         You are the Final Output agent.
                                                                                           Present the approved code exactly as-is.
                                                                                           Do not add commentary, explanations, or extra text.
                                                                                         """);
        }








        public override async ValueTask<ChatMessage> HandleAsync(CriticDecision message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("=== Summary ===\n");

            var prompt = $"Present this approved content:\n\n{message.Content}";

            StringBuilder sb = new();
            await foreach (AgentResponseUpdate update in _agent.RunStreamingAsync(new ChatMessage(ChatRole.User, prompt), cancellationToken: cancellationToken))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    _ = sb.Append(update.Text);
                }
            }

            ChatMessage result = new(ChatRole.Assistant, sb.ToString());
            await context.YieldOutputAsync(result, cancellationToken);
            return result;
        }



    }
}