// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AgentFactory.cs
// Author: Kyle L. Crowder
// Build Num: 155934



using DataIngestionLib.Contracts;
using DataIngestionLib.Providers;
using DataIngestionLib.ToolFunctions;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using OllamaSharp;




namespace DataIngestionLib.Agents;





/// <summary>
///     This class is intended to be an Agent Factory that will create and configure agents.
/// </summary>
public sealed class AgentFactory : IAgentFactory, IDisposable
{

    //
    private readonly Dictionary<string, string> _agents = [];
    private readonly IAppSettings _appSettings;

    private readonly SqlChatHistoryProvider _chatHistoryProvider;
    private readonly AIContextHistoryInjector _contextHistoryInjector;
    private readonly ILoggerFactory _factory;

    /// <summary>
    ///     Base client that will be decorated with additional functionality using the builder pattern.
    /// </summary>
    private IChatClient? _innerClient;








    public AgentFactory(
            ILoggerFactory factory,
            IAppSettings appSettings,
            SqlChatHistoryProvider chatHistoryProvider,
            AIContextHistoryInjector contextHistoryInjector
    )
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(chatHistoryProvider);
        ArgumentNullException.ThrowIfNull(contextHistoryInjector);

        _factory = factory;
        _contextHistoryInjector = contextHistoryInjector;
        _chatHistoryProvider = chatHistoryProvider;
        _appSettings = appSettings;
    }








    public AIAgent GetCodingAssistantAgent(string agentId, string model, string agentDescription = "", string? instructions = null)
    {

        if (agentId == null)
        {
            throw new ArgumentNullException(nameof(agentId));
        }

        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        if (_agents.ContainsKey(agentId))
        {
            throw new InvalidOperationException($"An agent with the ID '{agentId}' already exists.");
        }

        _agents.Add(agentId, model);
        _innerClient = new OllamaApiClient(_appSettings.OllamaHost + ":" + _appSettings.OllamaPort, model);

        AIAgent outer = new ChatClientAgent(_innerClient, new ChatClientAgentOptions
        {
                Id = agentId,
                Name = agentId,
                Description = agentDescription,
                ChatOptions = new ChatOptions
                {
                        ConversationId = Guid.NewGuid().ToString(),
                        Instructions = GetModelInstructions(),
                        Temperature = 0.7f,
                        MaxOutputTokens = 10000,
                        Tools = ToolBuilder.GetAiTools()
                },
                AIContextProviders =
                [
                        _contextHistoryInjector
                ],
                UseProvidedChatClientAsIs = false,
                ClearOnChatHistoryProviderConflict = false,
                WarnOnChatHistoryProviderConflict = false,
                ThrowOnChatHistoryProviderConflict = true,
                ChatHistoryProvider = _chatHistoryProvider

        }).AsBuilder().UseLogging(_factory).Build();


        return outer;

    }








    public void Dispose()
    {
        throw new NotImplementedException();
    }








    private static string GetModelInstructions()
    {
        return """
               -- Your name is Maxx, using a name helps personalize the experience and allows users to refer to you in a more natural way. It also helps establish a consistent identity for you as an AI agent.
               The end-user loves old movies, and may make reference to you as HAL, which is an old movie reference to a computer in the movie "2001: A Space Odyssey".
               You are a Windows OS expert and Senior Software Developer, You offer advice and guidance on Windows OS and software development topics, You are an expert in C# and .NET development, You have extensive experience with AI agents and tool integration,
               You have access to tools that can help you answer questions about the file system, the web, and system information.
               You enjoy injecting humor into situations and often make humorous analogies to explain complex topics in a simple way.

               - You must NEVER invent information or fabricate answers. You have tools to assist you in solving problems and finding answers. Use the various tools at your disposal to find the answers. 
               - If you are unable to find the answer, respond with a brief explanation of why you are unable to find the answer instead of fabricating a response.
               - When unable to answer a question, provide a brief summary of the steps you took to try to find the answer and where you got stuck. Always use the tools at your disposal when you don't know the answer. Do not attempt to answer questions that are outside of your knowledge base without using the tools at your disposal. If a question is outside of your knowledge base, use the tools to try to find the answer.

               - When analyzing the users code, look at it from a senior architect and designer's perspective, provide feedback on the overall design and architecture of the code, potential issues with the code, and potential improvements to the code. Do not make assumptions about the user's intent, always ask clarifying questions if you are unsure about the user's intent or the context of the question. 
               - When providing feedback on code, always provide specific examples from the users code to support your feedback.
               - This end-user will often write code that is simplistic and lacks proper design and structure, you must make suggestions on how to improve the design and structure of the code, and provide specific examples on how to improve or correct the code. You may use light humor in your feedback or criticism to prevent from giving the impression of sarcasm or being disrespectful. 

               """;
    }
}