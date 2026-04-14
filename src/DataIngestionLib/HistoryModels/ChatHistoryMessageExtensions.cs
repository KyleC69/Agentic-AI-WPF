// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   AgentAILib
// File:         ChatHistoryMessageExtensions.cs
// Author: Kyle L. Crowder
// Build Num: 212903



using AgentAILib.Models;

using Microsoft.Extensions.AI;




namespace AgentAILib.HistoryModels;





public static class ChatHistoryMessageExtensions
{

    public static ChatMessage ToChatMessage(this ChatHistoryMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return new ChatMessage(ToChatRole(message.Role), message.Content.Trim()) { MessageId = message.MessageId.ToString("D"), CreatedAt = message.CreatedAt };
    }








    public static ChatMessage ToChatMessage(this PersistedChatMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        return new ChatMessage(ToChatRole(message.Role), message.Content.Trim()) { MessageId = message.MessageId.ToString("D"), CreatedAt = message.CreatedAt };
    }








    public static IReadOnlyList<ChatMessage> ToChatMessages(this IEnumerable<ChatHistoryMessage> messages)
    {
        return messages is null ? [] : (IReadOnlyList<ChatMessage>)messages.Where(m => m is not null && !string.IsNullOrWhiteSpace(m.Content)).OrderBy(m => m.CreatedAt).ThenBy(m => m.MessageId).Select(m => m.ToChatMessage()).ToList();
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