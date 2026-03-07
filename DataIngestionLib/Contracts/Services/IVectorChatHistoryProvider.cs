// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         IVectorChatHistoryProvider.cs
//   Author: Kyle L. Crowder



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts.Services;





public interface IVectorChatHistoryProvider
{
    IReadOnlyList<string> StateKeys { get; }


    string GetSessionDbKey(AgentSession session);


    ValueTask<IEnumerable<ChatMessage>> InvokingAsync(ChatHistoryProvider.InvokingContext context, CancellationToken cancellationToken);


    ValueTask InvokedAsync(ChatHistoryProvider.InvokedContext context, CancellationToken cancellationToken);


    object? GetService(Type serviceType, object? serviceKey);


    T? GetService<T>(T? serviceKey);
}