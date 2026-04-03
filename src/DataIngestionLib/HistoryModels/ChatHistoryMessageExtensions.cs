// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatHistoryMessageExtensions.cs
// Author: Kyle L. Crowder
// Build Num: 095146



using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.HistoryModels;





public static class ChatHistoryMessageExtensions
{

    public static ChatMessage ToChatMessage(this ChatHistoryMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new ChatMessage(ToChatRole(message.Role), message.Content.Trim()) { MessageId = message.MessageId.ToString("D"), CreatedAt = message.TimestampUtc };
    }








    public static ChatMessage ToChatMessage(this PersistedChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return new ChatMessage(ToChatRole(message.Role), message.Content.Trim()) { MessageId = message.MessageId.ToString("D"), CreatedAt = message.TimestampUtc };
    }








    public static IReadOnlyList<ChatMessage> ToChatMessages(this IEnumerable<ChatHistoryMessage>? messages)
    {
        return messages is null ? [] : (IReadOnlyList<ChatMessage>)messages.Where(m => m is not null && !string.IsNullOrWhiteSpace(m.Content)).OrderBy(m => m.TimestampUtc).ThenBy(m => m.CreatedAt).Select(m => m.ToChatMessage()).ToList();
    }








    private static ChatRole ToChatRole(string? role)
    {
        return role?.Trim().ToLowerInvariant() switch
        {
                "assistant" => ChatRole.Assistant,
                "system" => ChatRole.System,
                "tool" => ChatRole.Tool,
                "user" => ChatRole.User,
                _ => ChatRole.User
        };
    }
}