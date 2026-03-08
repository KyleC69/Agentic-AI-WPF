// 2026/03/07
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         RAGAIContextProvider.cs
//   Author: Kyle L. Crowder



using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using BaseMessageAIContextProvider = Microsoft.Agents.AI.MessageAIContextProvider;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;




namespace DataIngestionLib.Services;





public sealed class RAGAIContextProvider : BaseMessageAIContextProvider
{
    private readonly IReadOnlyList<IRagContextSource> _sources;








    public RAGAIContextProvider(IEnumerable<IRagContextSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.ToArray();
    }








    protected override async ValueTask<IEnumerable<ChatMessage>?> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        ChatHistory requestMessages = [.. context.RequestMessages
            .Select(m => new AIChatMessage(m.Role, m.Text))];
        if (_sources.Count == 0)
        {
            return [];
        }

        List<AIChatMessage> aggregatedContext = [];
        foreach (IRagContextSource source in _sources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ChatHistory sourceMessages = await source
                    .GetContextMessagesAsync(requestMessages, context.Session, cancellationToken)
                    .ConfigureAwait(false);

            if (sourceMessages.Count == 0)
            {
                continue;
            }

            aggregatedContext.AddRange(sourceMessages.Where(static message => !string.IsNullOrWhiteSpace(message.Text)));
        }

        return aggregatedContext
            .Where(m => !string.IsNullOrWhiteSpace(m.Text))
            .Select(m => new ChatMessage(m.Role, m.Text))
            .ToArray();
    }








    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}