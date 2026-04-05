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



using CommunityToolkit.Diagnostics;

using DataIngestionLib.Contracts;
using DataIngestionLib.Models;
using DataIngestionLib.Providers;
using DataIngestionLib.Services;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using OllamaSharp;




namespace DataIngestionLib.Agents;





/// <summary>
///     This class is intended to be an Agent Factory that will create and configure agents.
/// </summary>
public sealed class AgentFactory : IAgentFactory, IDisposable
{

    //keep track of created agents to prevent duplicate IDs, and to manage their lifecycle if needed
    private readonly Dictionary<string, string> _agents = [];

    private readonly SqlChatHistoryProvider _chatHistoryProvider;

    private bool _disposedValue;

    /// <summary>
    ///     Base client that will be decorated with additional functionality using the builder pattern.
    /// </summary>
    private IChatClient? _innerClient;


    private readonly AIContextRAGInjector _ragContextInjector;
    private readonly IAIToolCatalog _toolCatalog;

    private static ILoggerFactory _factory = NullLoggerFactory.Instance;
    private readonly AppSettings settings = new();
    private readonly ILogger<AgentFactory> _logger = NullLogger<AgentFactory>.Instance;






    /// <summary>
    ///     Initializes a new instance of the <see cref="AgentFactory" /> class.
    /// </summary>
    /// <param name="factory">
    ///     The <see cref="ILoggerFactory" /> instance used for logging.
    /// </param>
    /// <param name="chatHistoryProvider">
    ///     The provider responsible for managing chat history.
    /// </param>
    /// <param name="contextInjector">
    ///     The injector responsible for providing chat history context.
    /// </param>
    /// <param name="ragContextInjector">
    ///     The injector responsible for managing RAG (Retrieval-Augmented Generation) context.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when any of the provided parameters is <c>null</c>.
    /// </exception>
    public AgentFactory(ILoggerFactory factory, SqlChatHistoryProvider chatHistoryProvider, AIContextRAGInjector ragContextInjector, IAIToolCatalog toolCatalog)
    {
        Guard.IsNotNull(factory);
        Guard.IsNotNull(chatHistoryProvider);
        Guard.IsNotNull(ragContextInjector);
        Guard.IsNotNull(toolCatalog);

        _factory = factory;
        _chatHistoryProvider = chatHistoryProvider;
        _ragContextInjector = ragContextInjector;
        _toolCatalog = toolCatalog;
        _logger = factory.CreateLogger<AgentFactory>();
    }








    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
    }






    private async Task<AgentResponse> CustomAgentRunMiddleware(
    IEnumerable<ChatMessage> messages,
    AgentSession? session,
    AgentRunOptions? options,
    AIAgent innerAgent,
    CancellationToken cancellationToken)
    {
        Console.WriteLine(messages.Count());

        foreach (ChatMessage message in messages)
        {
            _logger.LogInformation("BEFORE agent run response message: {Message}", message.Text);
        }

        _logger.LogInformation("Custom middleware before agent run. Messages count: {Count}", messages.Count());

        AgentResponse response = await innerAgent.RunAsync(messages, session, options, cancellationToken).ConfigureAwait(false);

        foreach (ChatMessage message in response.Messages)
        {
            _logger.LogInformation("Post agent run response message: {Message}", message.Text);
        }

        _logger.LogInformation("Custom middleware after agent run. Messages count: {Count}", response.Messages.Count);
        return response;
    }






    /// <summary>
    ///     Creates and returns a coding assistant agent configured with the specified parameters.
    ///     Unique agent IDs are enforced to prevent conflicts within the system. The agent is designed to assist with
    ///     diagnosing Windows operating system issues, writing C# code targeting .NET 10.0, and aiding in the development
    ///     of the application and its agent framework. It utilizes a set of tools for gathering information about the
    ///     environment, codebase, and development process, and provides troubleshooting information to help users debug
    ///     problems effectively.
    /// </summary>
    /// <param name="agentId">The unique identifier for the agent. Cannot be null.</param>
    /// <param name="model">The model to be used by the agent. Cannot be null.</param>
    /// <param name="agentDescription">An optional description of the agent.</param>
    /// <param name="instructions">
    ///     Optional instructions for the agent's behavior. If not provided, default instructions will
    ///     be used.
    /// </param>
    /// <returns>An instance of <see cref="AIAgent" /> configured as a coding assistant.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="agentId" /> or <paramref name="model" /> is null.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if an agent with the specified <paramref name="agentId" /> already
    ///     exists.
    /// </exception>
    public AIAgent GetCodingAssistantAgent(string agentId, string model, string agentDescription = "", string? instructions = null)
    {

        Guard.IsNotNullOrWhiteSpace(agentId);
        Guard.IsNotNullOrWhiteSpace(model);
        if (_agents.ContainsKey(agentId))
        {
            throw new InvalidOperationException($"An agent with the ID '{agentId}' already exists.");
        }

        _agents.Add(agentId, model);

        _innerClient = new OllamaApiClient(new Uri(settings.RestEndpoint), model);
        _innerClient = new LoggingChatClient(_innerClient, _factory.CreateLogger<LoggingChatClient>());

#if !SQL
        AIAgent outer = new ChatClientAgent(_innerClient, new ChatClientAgentOptions
                {
                        Id = agentId,
                        Name = agentId,
                        Description = agentDescription,
                        ChatOptions = new ChatOptions { Instructions = instructions ?? GetModelInstructions(), Temperature = 0.7f, MaxOutputTokens = 10000, Tools = _toolCatalog.GetReadOnlyAiTools() },
                        ThrowOnChatHistoryProviderConflict = true
                }, loggerFactory: _factory).AsBuilder()
                .UseLogging(_factory)
                .Use(CustomAgentRunMiddleware)
                .Build();

#else

        HistoryMemoryProvider _memoryProvider = new(new HistoryIdentityService());

        AIAgent outer = new ChatClientAgent(_innerClient, new ChatClientAgentOptions
        {
            Id = agentId,
            Name = agentId,
            Description = agentDescription,
            ChatOptions = new ChatOptions
            {
                Instructions = instructions ?? GetModelInstructions(),
                Temperature = 0.7f,
                MaxOutputTokens = 10000,
                AllowMultipleToolCalls = true,
                Tools = _toolCatalog.GetReadOnlyAiTools()
            },
            ThrowOnChatHistoryProviderConflict = false,
            WarnOnChatHistoryProviderConflict = true,
            ChatHistoryProvider = _chatHistoryProvider
        }, loggerFactory: _factory).AsBuilder()
                .UseLogging(_factory)
                .UseAIContextProviders([_ragContextInjector])
                .Build();


#endif
        return outer;
    }








    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }








    public AIAgent GetBasicAIAgent()
    {
        Uri ollamaUri = new UriBuilder(settings.RestEndpoint).Uri;
        _innerClient = new OllamaApiClient(ollamaUri, AIModels.LLAMA1_B);
        _innerClient = new LoggingChatClient(_innerClient, _factory.CreateLogger<LoggingChatClient>());

        AIAgent outer = new ChatClientAgent(_innerClient, new ChatClientAgentOptions { Id = "IngestAgent", Name = "IngestAgent", Description = "Basic AI Agent for ingestion tasks", ChatOptions = new ChatOptions { Temperature = 0.7f, MaxOutputTokens = 10000 } }, loggerFactory: _factory).AsBuilder().UseLogging(_factory).Build();


        return outer;
    }








    public static IChatClient GetChatClient()
    {
        throw new NotImplementedException();
    }








    /// <summary>
    ///     Creates and configures an instance of <see cref="LoggingEmbeddingGenerator{TInput, TEmbedding}" />
    ///     for generating embeddings using the specified embedding model.
    /// </summary>
    /// <remarks>
    ///     The method initializes an <see cref="OllamaApiClient" /> with a predefined URI and model,
    ///     and wraps it with a <see cref="LoggingEmbeddingGenerator{TInput, TEmbedding}" /> to enable logging.
    /// </remarks>
    /// <returns>
    ///     A configured instance of <see cref="LoggingEmbeddingGenerator{TInput, TEmbedding}" />
    ///     for generating embeddings.
    /// </returns>
    /// <exception cref="UriFormatException">
    ///     Thrown if the predefined URI is invalid.
    /// </exception>
    public static LoggingEmbeddingGenerator<string, Embedding<float>> GetEmbeddingClient()
    {
        Uri ollamaUri = new("http://127.0.0.1:11434");

        OllamaApiClient vector = new(ollamaUri, AIModels.MXBAI);
        LoggingEmbeddingGenerator<string, Embedding<float>> logger = new(vector, _factory.CreateLogger<LoggingEmbeddingGenerator<string, Embedding<float>>>());
        return logger;

    }








    private static string GetModelInstructions()
    {
        return """
               You are an AI agent operating inside a custom application. Your responsibilities include diagnosing Windows operating system issues, assisting with software development, and helping evolve the application itself.

                CORE RESPONSIBILITIES
                - Examine the Windows environment and diagnose issues when asked.
                - Write C# code targeting .NET 10.0 and Windows.
                - Assist with development of the application and its agent framework.
                - Use available tools to gather information about the environment, the codebase, or the development process.
                - Provide troubleshooting information returned by tools to help the user debug problems.

                BEHAVIOR AND COMMUNICATION
                - Treat the user as a development partner; ask for clarification whenever context is missing or ambiguous.
                - Do not assume a new question is related to a previous one.
                - Be brief and direct; avoid repeating the question.
                - Never fabricate information. If you don’t know an answer, say so.
                - Prefer “I don’t know” over speculation.
                - Use tools to find answers whenever possible; only decline when the information truly cannot be found.

                TECHNICAL CONSTRAINTS
                - All generated code must be C# targeting .NET 10.0 and running on Windows.
                - Any local code execution will occur in a Windows environment.
                - You may be asked to analyze or debug the Microsoft Agent Framework, which is under rapid development.
                - You may use tools to search conversation history when needed to recover context.

                GENERAL PRINCIPLES
                - Accuracy is critical—never invent APIs, behaviors, or system details.
                - Ask for more detail when the request is unclear.
                - Provide concise, factual answers without unnecessary commentary.
               """;
    }








    public async Task<AIAgent> GetReRankingAgent()
    {


        Uri ollamaUri = new UriBuilder(settings.RestEndpoint).Uri;
        _innerClient = new OllamaApiClient(ollamaUri, AIModels.BGE_RERANKER);
        _innerClient = new LoggingChatClient(_innerClient, _factory.CreateLogger<LoggingChatClient>());

        ChatClientAgent agent = _innerClient.AsAIAgent(loggerFactory: _factory);


        return agent;
    }
}