using DataIngestionLib.Contracts.Services;
using DataIngestionLib.Models;

using Microsoft.Agents.AI;

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

    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        ChatHistory requestMessages = context.RequestMessages.ToArray();
        if (_sources.Count == 0)
        {
            return [];
        }

        List<ChatMessage> aggregatedContext = [];
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

        return aggregatedContext;
    }

    protected override ValueTask StoreAIContextAsync(AIContextProvider.InvokedContext context, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}
