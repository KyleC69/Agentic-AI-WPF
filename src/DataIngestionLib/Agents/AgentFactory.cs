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

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using OllamaSharp;




namespace DataIngestionLib.Agents;





/// <summary>
///     This class is intended to be an Agent Factory that will create and configure agents.
/// </summary>
public class AgentFactory : IAgentFactory
{
    private readonly SqlChatHistoryProvider _chatHistoryProvider;
    private readonly ILoggerFactory _factory;
    private readonly IHistoryIdentityService _historyIdentity;
    private readonly AIContextRAGInjector _ragContextInjector;
    private readonly IAppSettings _settings;
    private readonly IAIToolCatalog _toolCatalog;
    private readonly ILogger<AgentFactory> _logger;








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
    public AgentFactory(ILoggerFactory factory, SqlChatHistoryProvider chatHistoryProvider, AIContextRAGInjector ragContextInjector, IAIToolCatalog toolCatalog, IHistoryIdentityService historyIdentityService, IAppSettings appSettings)
    {
        Guard.IsNotNull(factory);
        Guard.IsNotNull(chatHistoryProvider);
        Guard.IsNotNull(ragContextInjector);
        Guard.IsNotNull(toolCatalog);
        Guard.IsNotNull(historyIdentityService);
        Guard.IsNotNull(appSettings);

        _historyIdentity = historyIdentityService;
        _settings = appSettings;
        _factory = factory;
        _chatHistoryProvider = chatHistoryProvider;
        _ragContextInjector = ragContextInjector;
        _toolCatalog = toolCatalog;
        _logger = factory.CreateLogger<AgentFactory>();
    }




    // Middleware that catches exceptions and provides graceful fallback responses
    private async Task<AgentResponse> ExceptionHandlingMiddleware(
            IEnumerable<ChatMessage> messages,
            AgentSession? session,
            AgentRunOptions? options,
            AIAgent innerAgent,
            CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogTrace("[ExceptionHandler] Executing agent run...");
            return await innerAgent.RunAsync(messages, session, options, cancellationToken);
        }
        catch (TimeoutException ex)
        {
            _logger.LogTrace($"[ExceptionHandler] Caught timeout: {ex.Message}");
            return new AgentResponse([new ChatMessage(ChatRole.Assistant,
                    "Sorry, the request timed out. Please try again later.")]);
        }
        catch (Exception ex)
        {
            _logger.LogTrace($"[ExceptionHandler] Caught error: {ex.Message}");
            return new AgentResponse([new ChatMessage(ChatRole.Assistant,
                    "An error occurred while processing your request.")]);
        }
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
    public AIAgent BuildAssistantAgent(IChatClient client, string agentId, string name, string agentDescription = "", string? instructions = null)
    {

        Guard.IsNotNullOrWhiteSpace(agentId);
        HistoryMemoryProvider _window = new(_historyIdentity);


        ChatClientAgent outer = client.AsAIAgent(new ChatClientAgentOptions
        {
            Id = agentId,
            Name = name,
            Description = agentDescription,
            ChatOptions = new ChatOptions
            {
                Instructions = instructions ?? GetModelInstructions(),
                Temperature = 0.7f,
                MaxOutputTokens = 10000,
                AllowMultipleToolCalls = true,
                Tools = _toolCatalog.GetReadOnlyAiTools()
            },
            ChatHistoryProvider = _chatHistoryProvider,
            AIContextProviders = [_window, _ragContextInjector!],
            WarnOnChatHistoryProviderConflict = true,
            ThrowOnChatHistoryProviderConflict = true
        });
        return outer.AsBuilder().Use(this.ExceptionHandlingMiddleware, null).UseLogging(_factory).Build();

    }








    public AIAgent BuildBasicAgent(IChatClient client, string agentId, string name, string agentDescription = "", string? instructions = null)
    {



        ChatClientAgent outer = client.AsAIAgent(new ChatClientAgentOptions
        {
            Id = agentId,
            Name = name,
            Description = agentDescription,
            ChatOptions = new ChatOptions
            {
                Instructions = instructions ?? GetModelInstructions(),
                Temperature = 0.7f,
                MaxOutputTokens = 10000,
                AllowMultipleToolCalls = true,
                Tools = _toolCatalog.GetReadOnlyAiTools()
            }
        });
        return outer.AsBuilder()
                .Use(this.ExceptionHandlingMiddleware, null)
                .UseLogging(_factory).Build();



    }








    public IChatClient GetChatClient(string model)
    {
        Uri ollamaUri = new UriBuilder(_settings.RestEndpoint).Uri;
        OllamaApiClient client = new(ollamaUri, model);


        IChatClient baseclient = new LoggingChatClient(client, _factory.CreateLogger<LoggingChatClient>());
        baseclient = new TokenAccountingMiddleware(baseclient, _factory.CreateLogger<TokenAccountingMiddleware>());
        baseclient = new ForensicChatClient(baseclient, _factory.CreateLogger<ForensicChatClient>());
        return baseclient;
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
    public IEmbeddingGenerator<string, Embedding<float>> GetEmbeddingClient()
    {
        Uri ollamaUri = new(_settings.RestEndpoint);

        IEmbeddingGenerator<string, Embedding<float>> client = new OllamaApiClient(ollamaUri, AIModels.MXBAI);

        return client;

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
}