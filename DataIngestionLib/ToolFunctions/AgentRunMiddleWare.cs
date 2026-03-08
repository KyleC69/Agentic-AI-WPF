// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         AgentRunMiddleWare.cs
//   Author: Kyle L. Crowder



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using ChatMessage = Microsoft.Extensions.AI.ChatMessage;




namespace DataIngestionLib.ToolFunctions;





internal class AgentRunMiddleWare
{

    private readonly IChatClient _baseAgent;
    private readonly ILoggerFactory _factory;








    public AgentRunMiddleWare(IChatClient baseAgent, ILoggerFactory factory)
    {
        _baseAgent = baseAgent;
        _factory = factory;
    }








    private async Task<AgentResponse> CustomAgentRunMiddleware(
            IEnumerable<ChatMessage> messages,
            AgentSession? thread,
            AgentRunOptions? options,
            AIAgent innerAgent,
            CancellationToken cancellationToken)
    {
        Console.WriteLine($"Input: {messages.Count()}");
        AgentResponse response = await innerAgent.RunAsync(messages, thread, options, cancellationToken).ConfigureAwait(false);
        Console.WriteLine($"Output: {response.Messages.Count}");
        return response;
    }








    private async Task<ChatResponse> CustomChatClientMiddleware(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options,
            IChatClient innerChatClient,
            CancellationToken cancellationToken)
    {
        Console.WriteLine($"Input: {messages.Count()}");
        ChatResponse response = await innerChatClient.GetResponseAsync(messages, options, cancellationToken);
        Console.WriteLine($"Output: {response.Messages.Count}");

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








    public void run2()
    {
    }
}