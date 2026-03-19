// Copyright (c) Your Organization. All rights reserved.



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;




namespace DataIngestionLib.Providers.ChatHistory;

/// <summary>
/// Configuration options for <see cref="SqlChatHistoryProvider"/>.
/// Bind from <c>appsettings.json</c> or environment variables.
/// </summary>
public sealed class SqlChatHistoryOptions
{
    /// <summary>
    /// ADO.NET connection string to the target SQL database.
    /// Required for any concrete data-access implementation.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Name of the table that stores chat messages.
    /// Defaults to <c>ChatHistoryMessages</c>.
    /// </summary>
    public string TableName { get; set; } = "ChatHistoryMessages";

    /// <summary>
    /// Command timeout in seconds for all SQL operations.
    /// Defaults to <c>30</c>.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of messages returned by <c>LoadHistoryAsync</c>.
    /// When set the SQL query adds a TOP / LIMIT clause to bound memory usage.
    /// <c>0</c> means no limit (default).
    /// </summary>
    public int MaxMessagesPerLoad { get; set; } = 0;

    /// <summary>
    /// Optional key used to store provider state in <see cref="AgentSession.StateBag"/>.
    /// When unset, defaults to the provider type name.
    /// </summary>
    public string? StateKey { get; set; }

    /// <summary>
    /// Initializes provider state for sessions that do not yet have one.
    /// This is where enterprise key dimensions should be set:
    /// ApplicationId, UserId, AgentId, ConversationId, ChannelId, and TenantId.
    /// </summary>
    public Func<AgentSession?, SqlChatHistoryProvider.ProviderState>? StateInitializer { get; set; }

    /// <summary>
    /// Optional filter applied to chat history messages before they are merged into the invocation request.
    /// Passed through to <see cref="ChatHistoryProvider"/>.
    /// </summary>
    public Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>>? ProvideOutputMessageFilter { get; set; }

    /// <summary>
    /// Optional filter applied to request messages before they are stored.
    /// Passed through to <see cref="ChatHistoryProvider"/>.
    /// </summary>
    public Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>>? StoreInputRequestMessageFilter { get; set; }

    /// <summary>
    /// Optional filter applied to response messages before they are stored.
    /// Passed through to <see cref="ChatHistoryProvider"/>.
    /// </summary>
    public Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>>? StoreInputResponseMessageFilter { get; set; }
}


/// <summary>
/// Extension methods for wiring <see cref="IChatHistoryProvider"/> into the
/// Microsoft Agents SDK dependency-injection container.
/// </summary>
public static class ChatHistoryServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IChatHistoryProvider"/> using the supplied
    /// <typeparamref name="TProvider"/> concrete type.
    ///
    /// <para>
    /// Usage in <c>Program.cs</c>:
    /// <code>
    /// builder.Services.AddSqlChatHistory&lt;DapperChatHistoryProvider&gt;(options =>
    /// {
    ///     options.ConnectionString     = builder.Configuration.GetConnectionString("ChatHistory")!;
    ///     options.CommandTimeoutSeconds = 60;
    ///     options.MaxMessagesPerLoad    = 100;
    /// });
    /// </code>
    /// </para>
    /// </summary>
    /// <typeparam name="TProvider">
    /// Concrete class derived from <see cref="SqlChatHistoryProvider"/>.
    /// </typeparam>
    public static IServiceCollection AddSqlChatHistory<TProvider>(
        this IServiceCollection services,
        Action<SqlChatHistoryOptions> configure)
        where TProvider : SqlChatHistoryProvider
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        services.TryAddSingleton<TProvider>();
        services.TryAddSingleton<IChatHistoryProvider>(sp => sp.GetRequiredService<TProvider>());
        services.TryAddSingleton<ChatHistoryProvider>(sp => sp.GetRequiredService<TProvider>());
        return services;
    }

    /// <summary>
    /// Registers <see cref="IChatHistoryProvider"/> with a factory delegate, useful
    /// for providers that require constructor arguments not in DI.
    /// </summary>
    public static IServiceCollection AddSqlChatHistory(
        this IServiceCollection services,
        Action<SqlChatHistoryOptions> configure,
        Func<IServiceProvider, IChatHistoryProvider> factory)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);
        ArgumentNullException.ThrowIfNull(factory);

        services.Configure(configure);
        services.TryAddSingleton(factory);
        services.TryAddSingleton<ChatHistoryProvider>(sp =>
        {
            var provider = sp.GetRequiredService<IChatHistoryProvider>();
            if (provider is not ChatHistoryProvider chatHistoryProvider)
            {
                throw new InvalidOperationException(
                    "Configured IChatHistoryProvider must inherit ChatHistoryProvider.");
            }

            return chatHistoryProvider;
        });
        return services;
    }
}
