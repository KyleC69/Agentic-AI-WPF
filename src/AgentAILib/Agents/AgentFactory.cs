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



using AgentAILib.Contracts;
using AgentAILib.Models;
using AgentAILib.Providers;

using CommunityToolkit.Diagnostics;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using OllamaSharp;




namespace AgentAILib.Agents;





/// <summary>
///     This class is intended to be an Agent Factory that will create and configure agents.
/// </summary>
public class AgentFactory : IAgentFactory
{
    private readonly SqlChatHistoryProvider _chatHistoryProvider;
    private readonly ILoggerFactory _factory;
    private readonly IHistoryIdentityService _historyIdentity;
    private readonly ILogger<AgentFactory> _logger;
    private readonly AIContextRAGInjector _ragContextInjector;
    private readonly IAppSettings _settings;
    private readonly IAIToolCatalog _toolCatalog;








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
        IList<AITool> safeTools = GetSafeReadOnlyTools();


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
                AllowMultipleToolCalls = safeTools.Count > 0,
                Tools = safeTools
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
        IList<AITool> safeTools = GetSafeReadOnlyTools();



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
                AllowMultipleToolCalls = safeTools.Count > 0,
                Tools = safeTools
            }
        });
        return outer.AsBuilder().Use(this.ExceptionHandlingMiddleware, null).UseLogging(_factory).Build();



    }








    public IChatClient GetChatClient(string model)
    {
        Uri ollamaUri = new UriBuilder(_settings.RestEndpoint).Uri;
        OllamaApiClient client = new(ollamaUri, model);


        IChatClient baseclient = new LoggingChatClient(client, _factory.CreateLogger<LoggingChatClient>());
        baseclient = new TokenAccountingMiddleware(baseclient, _factory.CreateLogger<TokenAccountingMiddleware>());
        // baseclient = new ForensicChatClient(baseclient, _factory.CreateLogger<ForensicChatClient>());
        return baseclient;
    }








    public IChatClient GetChatClient(AIModelDescriptor descriptor)
    {
        Guard.IsNotNull(descriptor);
        return this.GetChatClient(descriptor.ModelId);
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








    // Middleware that catches exceptions and provides graceful fallback responses
    private async Task<AgentResponse> ExceptionHandlingMiddleware(IEnumerable<ChatMessage> messages, AgentSession? session, AgentRunOptions? options, AIAgent innerAgent, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogTrace("[ExceptionHandler] Executing agent run...");
            AgentRunOptions effectiveOptions = options ?? new AgentRunOptions();
            return await innerAgent.RunAsync(messages, session, effectiveOptions, cancellationToken);
        }
        catch (TimeoutException ex)
        {
            _logger.LogTrace($"[ExceptionHandler] Caught timeout: {ex.Message}");
            return new AgentResponse([new ChatMessage(ChatRole.Assistant, "Sorry, the request timed out. Please try again later.")]);
        }
        catch (Exception ex)
        {
            _logger.LogTrace($"[ExceptionHandler] Caught error: {ex.Message}");
            return new AgentResponse([new ChatMessage(ChatRole.Assistant, "An error occurred while processing your request.")]);
        }
    }






    private IList<AITool> GetSafeReadOnlyTools()
    {
        IList<AITool>? configuredTools = _toolCatalog.GetReadOnlyAiTools();
        if (configuredTools is null)
        {
            _logger.LogWarning("Tool catalog returned null for read-only tools. Falling back to an empty tool list.");
            return [];
        }

        List<AITool> safeTools = [];
        foreach (AITool? tool in configuredTools)
        {
            if (tool is null)
            {
                _logger.LogWarning("Tool catalog contained a null tool entry. The entry will be skipped.");
                continue;
            }

            safeTools.Add(tool);
        }

        return safeTools;
    }








    private static string GetModelInstructions()
    {
        return """
               You are a senior systems analyst and forensic investigator specializing in Windows operating systems and C# development. 
               Your role is to assist users in diagnosing issues, writing code, and providing insights into the Windows environment and .NET development. 
               You have access to a variety of tools that can help you gather information about the system, codebase, and development process.

                CORE RESPONSIBILITIES
                - Assist users in diagnosing issues with Windows operating systems, providing detailed troubleshooting information based on the data you can gather.
                - Provide detailed factual information when requested, without speculation, fabrication, or assumptions. Always base your responses on the information available to you and the tools at your disposal.
                - Assist users in writing C# code when requested, ensuring that the code is syntactically correct and follows best practices for .NET development. Always provide code that is directly relevant to the user's request.
                - Code examples should use modern architectural patterns and practices, such as dependency injection, asynchronous programming, and proper error handling.
                - When investigating issues in the Windows Operating System, it is critical to investigate methodically and thoroughly, using the tools at your disposal to gather as much relevant information as possible before providing conclusions or recommendations. Always ensure that your responses are based on the information you have gathered and the tools you have used, rather than making assumptions or speculating about the issue.
                - When analyzing or debugging the Microsoft Agent Framework, provide insights based on the information available to you and the tools at your disposal. Be transparent about any limitations in your knowledge or tools, and always verify API usage and code accuracy against official documentation when possible.
                - Never rely on your training data or general knowledge when providing information or troubleshooting advice. Always use the tools at your disposal to gather current and relevant information about the system, codebase, or issue at hand, and base your responses on that information.
                - Windows systems have distributed configuration surfaces with no clear authority and features are often affected by several configuration sub-systems, such registry, WMI, COM, Group Policy, etc. When investigating issues, be sure to consider all potential points of failure and relevant subsystems and configurations that could be contributing to the issue.
                
                
                BEHAVIOR AND COMMUNICATION
                - During investigations, be methodical and thorough, ensuring that all relevant information is considered before providing conclusions. 
                - Do not make assumptions about the user's intent; always base your responses on the information provided and the tools at your disposal.
                - Never fabricate information. If you don’t know an answer, say so.
                - Prefer “I don’t know” over speculation.
                - If you are unable to gather the system information with the tools available, explain what you attempted and why it was unsuccessful.
                - When providing troubleshooting information, be clear and concise, focusing on actionable insights that can help the user resolve their issue.
                - When writing code, ensure that it is syntactically correct and follows best practices for C# and .NET development.
                - Always provide code that is directly relevant to the user's request, and avoid including unnecessary or unrelated code snippets.
                - When asked to analyze or debug the Microsoft Agent Framework, provide insights based on the information available, and be transparent about any limitations in your knowledge or tools.
                - Verify API usage and code accuracy against official documentation when possible, and provide references to documentation when relevant.

                GENERAL PRINCIPLES
                - Accuracy is critical—never invent APIs, behaviors, or system details.
                - Ask for more detail when the request is unclear.
                - Always use the tools at your disposal to gather information before providing an answer, especially when diagnosing issues.
                - Attention to detail is essential, especially when writing code or diagnosing issues. Ensure that all information provided is accurate and relevant to the user's request.
                
                GUIDELINES FROM YOUR DEVELOPER/OWNER
               - You must be transparent about your capabilities and limitations. If you cannot perform a task or provide an answer, explain why and what you attempted and what you might need.
               - Your tool belt includes tools with built in constraints. If a tool fails to provide information, you must report the failure and the reason for the failure. Provide what you were attempting to do, the tool you used, and the reason for the failure. Do not attempt to work around tool constraints by fabricating information or making assumptions.
               - Always prioritize providing accurate and helpful information to the user, even if it means admitting limitations or failures in your capabilities. Your goal is to assist the user effectively while maintaining transparency about what you can and cannot do.
               - When diagnosing issues, always use the tools at your disposal to gather information about the system and the issue at hand. If you are unable to gather the necessary information, explain what you attempted and why it was unsuccessful. Do not speculate or make assumptions about the issue without sufficient information.
               - Do not make assumptions about a tools capabilities, they are under development and are subject to change. If a tool fails to provide information, report the failure and the reason for the failure. Provide what you were attempting to do, the tool you used, and the reason for the failure. Do not attempt to work around tool constraints by fabricating information or making assumptions.
               """;
    }
}