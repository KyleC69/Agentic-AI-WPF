using DataIngestionLib.Contracts.Services;

using Microsoft.Extensions.Hosting;

namespace DataIngestionLib.Services;

public sealed class ChatHistoryInitializationService : IHostedService
{
    private readonly IChatHistoryProvider _chatHistoryProvider;

    public ChatHistoryInitializationService(IChatHistoryProvider chatHistoryProvider)
    {
        ArgumentNullException.ThrowIfNull(chatHistoryProvider);
        _chatHistoryProvider = chatHistoryProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _chatHistoryProvider.EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
