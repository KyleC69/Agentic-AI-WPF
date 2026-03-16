// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         SqlChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 155947





using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Providers;





public class SqlChatHistoryProvider : ChatHistoryProvider
{





    /// <inheritdoc />
    protected override ValueTask<IEnumerable<ChatMessage>> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return base.InvokingCoreAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {

        var msgs = context.RequestMessages;

        return base.ProvideChatHistoryAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return base.StoreChatHistoryAsync(context, cancellationToken);
    }
}