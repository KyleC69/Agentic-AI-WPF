// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using DataIngestionLib.Models;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.HistoryModels;





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