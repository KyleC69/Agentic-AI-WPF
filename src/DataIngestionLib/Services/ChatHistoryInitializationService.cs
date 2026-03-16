// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatHistoryInitializationService.cs
// Author: Kyle L. Crowder
// Build Num: 051927



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

        if (_chatHistoryProvider is not ISQLChatHistoryProvider sqlChatHistoryProvider)
        {
            return;
        }

        ChatHistorySessionSnapshot? sessionSnapshot = await sqlChatHistoryProvider.GetLatestSessionSnapshotAsync(cancellationToken).ConfigureAwait(false);
        if (sessionSnapshot is null)
        {
            return;
        }

        ChatHistorySessionState.SetStartupSession(sessionSnapshot.SessionId, sessionSnapshot.ConversationId);
    }








    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}