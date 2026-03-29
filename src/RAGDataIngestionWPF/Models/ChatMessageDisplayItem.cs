// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF
// File:         ChatMessageDisplayItem.cs
// Author: Kyle L. Crowder
// Build Num: 051952



using Microsoft.Extensions.AI;




namespace RAGDataIngestionWPF.Models;





/// <summary>
///     Represents a chat message formatted for WPF binding so the view can react to collection changes and display state
///     without depending on transport-specific message types.
/// </summary>
public sealed record ChatMessageDisplayItem(string Text, string Role, bool IsUser)
{
    /// <summary>
    ///     Creates a display item from an AI chat role and message text so the UI can bind to stable, presentation-focused
    ///     values.
    /// </summary>
    /// <param name="role">The role that authored the message.</param>
    /// <param name="text">The text to display for the message.</param>
    /// <returns>A message item shaped for the `MainPage` bindings.</returns>
    public static ChatMessageDisplayItem Create(ChatRole role, string text)
    {
        return new(text, role.ToString(), role == ChatRole.User);
    }
}