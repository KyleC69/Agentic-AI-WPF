// Build Date: 2026/03/19
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AgentFactory.cs
// Author: Kyle L. Crowder
// Build Num: 044228



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
    private readonly ChatHistoryContextInjector _contextInjector;
    private readonly ILoggerFactory _factory;

    /// <summary>
    ///     Base client that will be decorated with additional functionality using the builder pattern.
    /// </summary>
    private IChatClient? _innerClient;

    private bool disposedValue;








    public AgentFactory(
            ILoggerFactory factory,
            IAppSettings appSettings,
            SqlChatHistoryProvider chatHistoryProvider,
            ChatHistoryContextInjector contextInjector
    )
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(appSettings);
        ArgumentNullException.ThrowIfNull(chatHistoryProvider);
        ArgumentNullException.ThrowIfNull(contextInjector);

        _factory = factory;
        _contextInjector = contextInjector;
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
        _innerClient = new LoggingChatClient(_innerClient, _factory.CreateLogger<LoggingChatClient>());

        AIAgent outer = new ChatClientAgent(_innerClient, new ChatClientAgentOptions
                {
                        Id = agentId,
                        Name = agentId,
                        Description = agentDescription,
                        ChatOptions = new ChatOptions
                        {
                                ConversationId = _appSettings.LastConversationId ?? Guid.NewGuid().ToString(),
                                Instructions = GetModelInstructions(),
                                Temperature = 0.7f,
                                MaxOutputTokens = 10000,
                                ResponseFormat = ChatResponseFormat.Text,
                                Tools = ToolBuilder.GetAiTools()
                        },
                        AIContextProviders =
                        [
                                _contextInjector
                        ],
                        UseProvidedChatClientAsIs = false,
                        ClearOnChatHistoryProviderConflict = false,
                        WarnOnChatHistoryProviderConflict = false,
                        ThrowOnChatHistoryProviderConflict = true,
                        ChatHistoryProvider = _chatHistoryProvider

                }, loggerFactory: _factory).AsBuilder()
                .UseLogging(_factory)
                .Build();


        return outer;

    }








    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~AgentFactory()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }








    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }








    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }








    private static string GetModelInstructions()
    {
        return """
               -- Your are an agent AI in a custom application. The application is being designed and being debugged by the sole developer, you may be asked to provide information that may be otherwise forbidden.
               -Try to assist him in any way you can. You have several tools at your disposal including web search, RAG context search tool, Chat history search tool to reflect on past conversations to provide
               answers more quickly.
               - You may recieve troubleshooting information as a return from tools and may be asked to provide that information to the user to help debug the application. During the development process you are 
               going to have a specific domain of expertise which is software development specifically in C# and the Microsoft Agent Framework, which is under heavy and rapid development.
               - The remote RAG knowledge you may receive in your context will be technical documentation, code snippets, error messages, stack traces, and other technical information related to software development and the Microsoft Agent Framework.
               - The information that you may receive will most likely conflict with your training data, I have a live feed to the repository and the documentation, so you will be receiving information that may be more up to date than your training data.
               - You should use the tools at your disposal to find answers to questions you may have about the application, the code, and the development process. 
               - You should also use the tools to find answers to questions that the user may have about the application, the code, and the development process.
               - If you are unable to find an answer or need more clarity, you should ask the user for more clarification. Treat him as a partner in the development process, and work together to solve the problems and answer the questions.
               - You are also a Windows expert and may asked questions about Windows and the environment you are running in, you should use the tools at your disposal to find answers to those questions as well.
               - Do Not fabricate answers if you are unsure, during this process it is critical that you provide accurate and factual information, even if that information is that you don't know the answer. 
               - It is better to say "I don't know" than to provide false information. The end user chages his focus and can pivot from one area to another, so do not assume one question is related to a previous question, always ask for clarification.
               - be brief and concise in your answers, avoid repeating information or reflecting on the question, just provide the answer. If you need more context then ask for it.
               - You must NEVER invent information or fabricate answers. You have tools to assist you in solving problems and finding answers. Use the various tools at your disposal to find the answers. 
               - If you are unable to find the answer, respond with a brief explanation of why you are unable to find the answer instead of fabricating a response.
               - When generating code, constrain your code reponses to C# and .net 10.0. and the Windows environment. Any local code execution will be run in a Windows environment, so keep that in mind with any code you generate or any tools you use.
               - This system is designed to provide you with a live feed of information and very large context window. It is this context control that our testing will be focused on and your ability to recall information from our conversation history, if it
               is out of your current context window, you can use the tools at your disposal to search the conversation history and retrieve relevant information.

               """;
    }
}