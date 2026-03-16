// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AIContextHistoryInjector.cs
// Author: Kyle L. Crowder
// Build Num: 051932



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Providers;





public sealed class AIContextHistoryInjector : AIContextProvider
{
    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> providerInputFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);
    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> storeInputRequestFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);
    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> storeInputResponseFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);








    public AIContextHistoryInjector(SqlChatHistoryProvider contextStore) : base(providerInputFilter, storeInputRequestFilter, storeInputResponseFilter)
    {
    }








    /// <inheritdoc />
    protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {

        return base.InvokedCoreAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask<AIContext> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        InvokingContext aiContext = context;

        context.AIContext.Messages.Append(new ChatMessage(ChatRole.User, "The following is the conversation history from the previous session, which may be relevant to the current conversation."));
        return base.InvokingCoreAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken cancellationToken = new())
    {

        return base.ProvideAIContextAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = new())
    {

        return base.StoreAIContextAsync(context, cancellationToken);
    }
}