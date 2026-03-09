// 2026/03/08
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         AgentRunMiddleWare.cs
//   Author: Kyle L. Crowder



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using ChatMessage = Microsoft.Extensions.AI.ChatMessage;




namespace DataIngestionLib.ToolFunctions;





public class AgentRunMiddleWare
{

    private readonly IChatClient _baseAgent;
    private readonly ILoggerFactory _factory;
    private readonly ILogger<AgentRunMiddleWare> _logger;








    public AgentRunMiddleWare(IChatClient baseAgent, ILoggerFactory factory, ILogger<AgentRunMiddleWare> logger)
    {
        _baseAgent = baseAgent;
        _factory = factory;
        _logger = logger;
    }








    private async Task<AgentResponse> CustomAgentRunMiddleware(
            IEnumerable<ChatMessage> messages,
            AgentSession? thread,
            AgentRunOptions? options,
            AIAgent innerAgent,
            CancellationToken cancellationToken)
    {
        _logger.LogDebug("Input: {MessageCount}", messages.Count());
        AgentResponse response = await innerAgent.RunAsync(messages, thread, options, cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Output: {MessageCount}", response.Messages.Count);
        return response;
    }








    private async Task<ChatResponse> CustomChatClientMiddleware(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options,
            IChatClient innerChatClient,
            CancellationToken cancellationToken)
    {
        _logger.LogDebug("Input: {MessageCount}", messages.Count());
        ChatResponse response = await innerChatClient.GetResponseAsync(messages, options, cancellationToken);
        _logger.LogDebug("Output: {MessageCount}", response.Messages.Count);

        return response;
    }








    internal void Run()
    {

        IChatClient outer = new ChatClientBuilder(_baseAgent)
                .UseLogging(_factory)
                .UseFunctionInvocation().Build();


        ChatOptions options = new()
        {
                Tools = ToolBuilder.GetAiTools(),
                Instructions = """

                               """

        };








    }








}