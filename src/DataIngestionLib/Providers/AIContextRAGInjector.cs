// Build Date: 2026/03/29
// Solution: File
// Project:   DataIngestionLib
// File:         AIContextRAGInjector.cs
// Author: Kyle L. Crowder
// Build Num: 051930



using DataIngestionLib.Contracts.Services;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Providers;





public sealed class AIContextRAGInjector : MessageAIContextProvider
{
    private readonly IReadOnlyList<IRagContextSource> _sources;








    public AIContextRAGInjector(IEnumerable<IRagContextSource> sources)
    {
        ArgumentNullException.ThrowIfNull(sources);
        _sources = sources.ToArray();
    }








    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(context);

        List<ChatMessage> requestMessages =
        [
                .. context.RequestMessages.Select(m => new ChatMessage(m.Role, m.Text))
        ];
        if (_sources.Count == 0)
        {
            return [];
        }

        List<ChatMessage> aggregatedContext = [];
        foreach (IRagContextSource source in _sources)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourceMessages = await source.GetContextMessagesAsync(requestMessages, context.Session, cancellationToken).ConfigureAwait(false);

            if (sourceMessages.Count == 0)
            {
                continue;
            }

            aggregatedContext.AddRange(sourceMessages.Where(static message => !string.IsNullOrWhiteSpace(message.Text)));
        }

        return aggregatedContext.Where(m => !string.IsNullOrWhiteSpace(m.Text)).Select(m => new ChatMessage(m.Role, m.Text)).ToArray();
    }








    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}