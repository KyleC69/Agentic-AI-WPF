// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         IVectorChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 051920



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IVectorChatHistoryProvider
{
    IReadOnlyList<string> StateKeys { get; }


    object? GetService(Type serviceType, object? serviceKey);


    T? GetService<T>(T? serviceKey);


    string GetSessionDbKey(AgentSession session);


    ValueTask InvokedAsync(ChatHistoryProvider.InvokedContext context, CancellationToken cancellationToken);


    ValueTask<IEnumerable<ChatMessage>> InvokingAsync(ChatHistoryProvider.InvokingContext context, CancellationToken cancellationToken);
}