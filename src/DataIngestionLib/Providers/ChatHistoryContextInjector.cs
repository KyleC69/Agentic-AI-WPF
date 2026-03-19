// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatHistoryContextInjector.cs
// Author: Kyle L. Crowder
// Build Num: 051932



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public sealed class ChatHistoryContextInjector : AIContextProvider
{
    private readonly ILogger<ChatHistoryContextInjector> _logger;
    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> providerInputFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);
    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> storeInputRequestFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);
    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> storeInputResponseFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);








    public ChatHistoryContextInjector(ILogger<ChatHistoryContextInjector> logger)
        : base(providerInputFilter, storeInputRequestFilter, storeInputResponseFilter)
    {
        _logger = logger;
    }


    





    /// <summary>
    /// Asynchronously handles the core logic for an AI operation invocation.
    /// </summary>
    /// <param name="context">
    /// The <see cref="InvokedContext"/> containing details about the AI operation being invoked.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// This method is responsible for processing the invocation context and performing any necessary actions
    /// before delegating to the base implementation.
    /// </remarks>
    protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var lastRequest = context.RequestMessages?.LastOrDefault();
        string messageId = lastRequest is null
            ? string.Empty
            : lastRequest.GetAgentRequestMessageSourceId() ?? string.Empty;
        string conversationId = context.Session?.StateBag?.GetValue<string>("ConversationId") ?? string.Empty;

        _logger.LogTrace(
            "Call from InvokedCore in ChatHistoryContextInjector: MessageID {MessageId} ConversationID {ConversationId}",
            messageId,
            conversationId);

        return base.InvokedCoreAsync(context, cancellationToken);
    }








    /// <summary>
    /// Asynchronously prepares and provides the AI context for an AI operation before it is invoked.
    /// </summary>
    /// <param name="context">
    /// The <see cref="InvokingContext"/> containing details about the AI operation being prepared.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation, with the result being the prepared <see cref="AIContext"/>.
    /// </returns>
    /// <remarks>
    /// This method is responsible for initializing and returning the AI context required for the AI operation.
    /// </remarks>
    protected override ValueTask<AIContext> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        string messageId = context.AIContext?.Messages?.LastOrDefault()?.MessageId ?? string.Empty;
        string conversationId = context.Session?.StateBag?.GetValue<string>("ConversationId") ?? string.Empty;

        _logger.LogTrace(
            "Call from InvokingCoreAsync in ChatHistoryContextInjector: MessageID {MessageId} ConversationID {ConversationId}",
            messageId,
            conversationId);

                if (context.AIContext?.Messages != null)
        {
      //      context.AIContext.Messages.Append(new ChatMessage(ChatRole.User, "The following is the conversation history from the previous session, which may be relevant to the current conversation."));
        }

        return base.InvokingCoreAsync(context, cancellationToken);
    }








    /// <summary>
    /// Asynchronously provides the AI context for the current invocation of an AI operation.
    /// </summary>
    /// <param name="context">
    /// The <see cref="InvokingContext"/> containing details about the AI operation being invoked.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> representing the asynchronous operation, with the result being the provided <see cref="AIContext"/>.
    /// </returns>
    /// <remarks>
    /// This method is responsible for preparing and returning the AI context required for the AI operation.
    /// </remarks>
    protected override ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken cancellationToken = new())
    {
        string messageId = context.Session?.StateBag?.GetValue<string>("MessageId") ?? string.Empty;
        string conversationId = context.Session?.StateBag?.GetValue<string>("ConversationId") ?? string.Empty;

        _logger.LogTrace(
            "Call from ProvideAIContextAsync in ChatHistoryContextInjector: MessageID {MessageId} ConversationID {ConversationId}",
            messageId,
            conversationId);
        
        return base.ProvideAIContextAsync(context, cancellationToken);
    }








    /// <summary>
    /// Asynchronously stores the AI context after the invocation of an AI operation.
    /// </summary>
    /// <param name="context">
    /// The <see cref="InvokedContext"/> containing details about the AI operation that was invoked.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// A <see cref="ValueTask"/> representing the asynchronous operation.
    /// </returns>
    /// <remarks>
    /// This method is invoked to persist any relevant AI context after the operation has been processed.
    /// </remarks>
    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = new())
    {
        string messageId = context.Session?.StateBag?.GetValue<string>("MessageId") ?? string.Empty;
        string conversationId = context.Session?.StateBag?.GetValue<string>("ConversationId") ?? string.Empty;

        _logger.LogTrace(
            "Call from StoreAIContextAsync in ChatHistoryContextInjector: MessageID {MessageId} ConversationID {ConversationId}",
            messageId,
            conversationId);
      
        return base.StoreAIContextAsync(context, cancellationToken);
    }
}