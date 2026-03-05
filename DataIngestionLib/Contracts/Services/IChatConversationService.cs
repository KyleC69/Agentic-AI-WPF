using DataIngestionLib.Models;

namespace DataIngestionLib.Contracts.Services;

public interface IChatConversationService
{
    /// <summary>
    /// Loads the persisted chat session for the current local user profile.
    /// </summary>
    /// <returns>The loaded chat session or a new empty session when no persisted state is available.</returns>
    ChatSessionState LoadSession();

    /// <summary>
    /// Persists the provided chat session state to local storage.
    /// </summary>
    /// <param name="sessionState">The session state to persist.</param>
    void SaveSession(ChatSessionState sessionState);

    /// <summary>
    /// Creates a user chat message with normalized formatting and token metadata.
    /// </summary>
    /// <param name="content">The raw user input content.</param>
    /// <returns>A normalized user chat message.</returns>
    ChatMessage CreateUserMessage(string content);

    /// <summary>
    /// Creates an assistant chat message with normalized formatting and token metadata.
    /// </summary>
    /// <param name="content">The assistant response content.</param>
    /// <returns>A normalized assistant chat message.</returns>
    ChatMessage CreateAssistantMessage(string content);

    /// <summary>
    /// Generates an assistant response message asynchronously for the supplied user message.
    /// </summary>
    /// <param name="userMessage">The user message content to answer.</param>
    /// <param name="contextTokenCount">The active context token count at generation time.</param>
    /// <param name="cancellationToken">The cancellation token for interrupting generation.</param>
    /// <returns>The generated assistant chat message.</returns>
    Task<ChatMessage> GenerateAssistantMessageAsync(string userMessage, int contextTokenCount, CancellationToken cancellationToken);

    /// <summary>
    /// Appends a chat message into history and context while enforcing the configured sliding token window.
    /// </summary>
    /// <param name="sessionState">The current session state.</param>
    /// <param name="message">The message to append.</param>
    /// <returns>The updated session state after enforcing sliding context rules.</returns>
    ChatSessionState AppendMessage(ChatSessionState sessionState, ChatMessage message);
}
