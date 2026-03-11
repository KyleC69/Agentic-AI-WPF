// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         ChatHistory.cs
// Author: Kyle L. Crowder
// Build Num: 105643



using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.AI;




namespace DataIngestionLib.Models;





/// <summary>
///     Represents a mutable list of <see cref="AIChatMessage" /> values with role-focused helpers for agent conversations.
/// </summary>
/// <remarks>
///     This type is intended to be directly consumed by Agent Framework and <see cref="IChatClient" /> APIs that operate
///     on <see cref="AIChatMessage" /> sequences. Keep chat manipulation helpers here to avoid duplicating message logic
///     in
///     service layers.
/// </remarks>
public sealed class ChatHistory : IList<AIChatMessage>, IReadOnlyList<AIChatMessage>, INotifyCollectionChanged, INotifyPropertyChanged
{
    private readonly List<AIChatMessage> _messages;








    /// <summary>
    ///     Initializes an empty chat history.
    /// </summary>
    public ChatHistory()
    {
        _messages = [];
    }








    public ChatHistory(IEnumerable<(ChatRole Role, string Text)> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        _messages = [];
        foreach ((ChatRole Role, var Text) in messages) Add(new AIChatMessage(Role, Text));
    }








    /// <summary>
    ///     Initializes chat history with a single message.
    /// </summary>
    /// <param name="message">The text message to add to the first message in chat history.</param>
    /// <param name="role">The role to add as the first message.</param>
    public ChatHistory(string message, ChatRole role)
    {
        EnsureNotNullOrWhiteSpace(message, nameof(message));

        _messages = [new AIChatMessage(role, message)];
    }








    /// <summary>
    ///     Initializes chat history with a single system message.
    /// </summary>
    /// <param name="systemMessage">The system message to add to the history.</param>
    public ChatHistory(string systemMessage)
            : this(systemMessage, ChatRole.System)
    {
    }








    /// <summary>
    ///     Initializes a new instance of the <see cref="ChatHistory" /> class with a collection of context request messages.
    /// </summary>
    /// <param name="contextRequestMessages">
    ///     A collection of <see cref="AIChatMessage" /> instances representing the context request messages.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="contextRequestMessages" /> is <c>null</c>.
    /// </exception>
    public ChatHistory(IEnumerable<AIChatMessage> contextRequestMessages)
    {
        ArgumentNullException.ThrowIfNull(contextRequestMessages);
        _messages = contextRequestMessages.Select(msg => new AIChatMessage(msg.Role, msg.Text)).ToList();
    }








    /// <summary>
    ///     Gets the newest message in the history, or <see langword="null" /> when history is empty.
    /// </summary>
    public AIChatMessage? LastMessage
    {
        get { return _messages.Count == 0 ? null : _messages[^1]; }
    }





    /// <summary>
    ///     Gets the number of messages in the history.
    /// </summary>
    public int Count
    {
        get { return _messages.Count; }
    }








    /// <summary>
    ///     Adds a message to the history.
    /// </summary>
    /// <param name="item">The message to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
    public void Add(AIChatMessage item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _messages.Add(item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
    }








    /// <summary>
    ///     Inserts a message into the history at the specified index.
    /// </summary>
    /// <param name="index">The index at which the item should be inserted.</param>
    /// <param name="item">The message to insert.</param>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
    public void Insert(int index, AIChatMessage item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _messages.Insert(index, item);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }








    /// <summary>
    ///     Copies all of the messages in the history to an array, starting at the specified destination array index.
    /// </summary>
    /// <param name="array">The destination array into which the messages should be copied.</param>
    /// <param name="arrayIndex">The zero-based index into <paramref name="array" /> at which copying should begin.</param>
    /// <exception cref="ArgumentNullException"><paramref name="array" /> is null.</exception>
    /// <exception cref="ArgumentException">
    ///     The number of messages in the history is greater than the available space from
    ///     <paramref name="arrayIndex" /> to the end of <paramref name="array" />.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex" /> is less than 0.</exception>
    public void CopyTo(AIChatMessage[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        _messages.CopyTo(array, arrayIndex);
    }








    /// <summary>
    ///     Removes all messages from the history.
    /// </summary>
    public void Clear()
    {
        _messages.Clear();
    }








    /// <summary>
    ///     Gets or sets the message at the specified index in the history.
    /// </summary>
    /// <param name="index">The index of the message to get or set.</param>
    /// <returns>The message at the specified index.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index" /> was not valid for this history.</exception>
    public AIChatMessage this[int index]
    {
        get { return _messages[index]; }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _messages[index] = value;
        }
    }








    /// <summary>
    ///     Determines whether a message is in the history.
    /// </summary>
    /// <param name="item">The message to locate.</param>
    /// <returns><see langword="true" /> if the message is found in the history; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
    public bool Contains(AIChatMessage item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _messages.Contains(item);
    }








    /// <summary>
    ///     Searches for the specified message and returns the index of the first occurrence.
    /// </summary>
    /// <param name="item">The message to locate.</param>
    /// <returns>The index of the first found occurrence of the specified message; -1 if the message could not be found.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
    public int IndexOf(AIChatMessage item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _messages.IndexOf(item);
    }








    /// <summary>
    ///     Removes the message at the specified index from the history.
    /// </summary>
    /// <param name="index">The index of the message to remove.</param>
    public void RemoveAt(int index)
    {
        _messages.RemoveAt(index);
    }








    /// <summary>
    ///     Removes the first occurrence of the specified message from the history.
    /// </summary>
    /// <param name="item">The message to remove from the history.</param>
    /// <returns><see langword="true" /> if the item was successfully removed; otherwise, <see langword="false" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item" /> is null.</exception>
    public bool Remove(AIChatMessage item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return _messages.Remove(item);
    }








    /// <inheritdoc />
    bool ICollection<AIChatMessage>.IsReadOnly
    {
        get { return false; }
    }








    /// <inheritdoc />
    IEnumerator<AIChatMessage> IEnumerable<AIChatMessage>.GetEnumerator()
    {
        return _messages.GetEnumerator();
    }








    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _messages.GetEnumerator();
    }








    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;





    public event PropertyChangedEventHandler? PropertyChanged;








    public void Add(ChatRole mRole, string mText)
    {
        switch (mRole.Value)
        {
            case "user":
                AddUserMessage(mText);
                break;
            case "assistant":
                AddAssistantMessage(mText);
                break;
            case "system":
                AddSystemMessage(mText);
                break;
        }


    }








    /// <summary>
    ///     Adds an assistant message to the chat history.
    /// </summary>
    /// <param name="content">Message content.</param>
    public void AddAssistantMessage(string content)
    {
        AddMessage(ChatRole.Assistant, content);
    }








    /// <summary>
    ///     Adds multiple assistant messages to chat history.
    /// </summary>
    /// <param name="messages">Messages to add, all with <see cref="ChatRole.Assistant" /> role.</param>
    public void AddAssistantMessages(IEnumerable<AIChatMessage> messages)
    {
        AddMessagesByRole(messages, ChatRole.Assistant, nameof(messages));
    }








    /// <summary>
    ///     Adds a message using role and text content.
    /// </summary>
    /// <param name="authorRole">Role of the message author.</param>
    /// <param name="content">Message text content.</param>
    public void AddMessage(ChatRole authorRole, string content)
    {
        EnsureNotNullOrWhiteSpace(content, nameof(content));
        Add(new AIChatMessage(authorRole, content));
    }








    private void AddMessagesByRole(IEnumerable<AIChatMessage> messages, ChatRole expectedRole, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(messages);

        foreach (AIChatMessage message in messages)
        {
            ArgumentNullException.ThrowIfNull(message);

            if (message.Role != expectedRole)
            {
                throw new ArgumentException($"All messages must have role '{expectedRole.Value}'.", parameterName);
            }

            Add(message);
        }
    }








    /// <summary>
    ///     Adds messages to the history.
    /// </summary>
    /// <param name="items">The collection whose messages should be added to the history.</param>
    /// <exception cref="ArgumentNullException"><paramref name="items" /> is null.</exception>
    public void AddRange(IEnumerable<AIChatMessage> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        foreach (AIChatMessage item in items) Add(item);
    }








    /// <summary>
    ///     Adds a system message to the chat history.
    /// </summary>
    /// <param name="content">Message content.</param>
    public void AddSystemMessage(string content)
    {
        AddMessage(ChatRole.System, content);
    }








    /// <summary>
    ///     Adds multiple system messages to chat history.
    /// </summary>
    /// <param name="messages">Messages to add, all with <see cref="ChatRole.System" /> role.</param>
    public void AddSystemMessages(IEnumerable<AIChatMessage> messages)
    {
        AddMessagesByRole(messages, ChatRole.System, nameof(messages));
    }








    /// <summary>
    ///     Adds a user message to the chat history.
    /// </summary>
    /// <param name="content">Message content.</param>
    public void AddUserMessage(string content)
    {
        AddMessage(ChatRole.User, content);
    }








    /// <summary>
    ///     Adds multiple user messages to chat history.
    /// </summary>
    /// <param name="messages">Messages to add, all with <see cref="ChatRole.User" /> role.</param>
    public void AddUserMessages(IEnumerable<AIChatMessage> messages)
    {
        AddMessagesByRole(messages, ChatRole.User, nameof(messages));
    }








    private static void EnsureNotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", paramName);
        }
    }








    /// <summary>
    ///     Estimates token count from newest to oldest using a max token budget.
    /// </summary>
    /// <param name="maxTokens">Maximum allowed tokens in the context window.</param>
    /// <returns>Estimated token count that fits in the configured context window.</returns>
    public int EstimateContextTokenCount(int maxTokens)
    {
        if (maxTokens <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTokens), "Maximum context tokens must be a positive value.");
        }

        var tokenCount = 0;

        for (var index = _messages.Count - 1; index >= 0; index--)
        {
            var text = _messages[index].Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            var messageTokenCount = Math.Max(1, text.Length / 4);
            if (tokenCount + messageTokenCount > maxTokens)
            {
                break;
            }

            tokenCount += messageTokenCount;
        }

        return tokenCount;
    }








    /// <summary>
    ///     Estimates token count using a simple 4-chars-per-token heuristic.
    /// </summary>
    /// <returns>Estimated token count for all messages.</returns>
    public int EstimateTokenCount()
    {
        var tokenCount = 0;

        foreach (AIChatMessage message in _messages)
        {
            var text = message.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                continue;
            }

            tokenCount += Math.Max(1, text.Length / 4);
        }

        return tokenCount;
    }








    /// <summary>
    ///     Gets the text from the most recent message for the provided role.
    /// </summary>
    /// <param name="role">Role to search for.</param>
    /// <returns>The message text when found; otherwise an empty string.</returns>
    public string GetLastMessageText(ChatRole role)
    {
        return TryGetLastMessage(role, out AIChatMessage? message)
                ? message.Text
                : string.Empty;
    }








    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }








    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }








    /// <summary>
    ///     Implicitly converts a <see cref="AIChatMessage" /> array to a <see cref="ChatHistory" /> instance.
    /// </summary>
    /// <param name="messages">The messages to include in the history.</param>
    public static implicit operator ChatHistory(AIChatMessage[] messages)
    {
        ArgumentNullException.ThrowIfNull(messages);
        return [.. messages];
    }








    /// <summary>
    ///     Implicitly converts a <see cref="ChatHistory" /> instance to a <see cref="AIChatMessage" /> array.
    /// </summary>
    /// <param name="history">The history to convert.</param>
    public static implicit operator AIChatMessage[](ChatHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        return [.. history._messages];
    }








    /// <summary>
    ///     Removes a range of messages from the history.
    /// </summary>
    /// <param name="index">The index of the range of elements to remove.</param>
    /// <param name="count">The number of elements to remove.</param>
    public void RemoveRange(int index, int count)
    {
        _messages.RemoveRange(index, count);
    }








    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }








    /// <summary>
    ///     Attempts to get the most recent message matching the provided role.
    /// </summary>
    /// <param name="role">Role to search for.</param>
    /// <param name="message">The latest matching message when found; otherwise <see langword="null" />.</param>
    /// <returns><see langword="true" /> when a matching message exists; otherwise <see langword="false" />.</returns>
    public bool TryGetLastMessage(ChatRole role, [NotNullWhen(true)] out AIChatMessage? message)
    {
        for (var index = _messages.Count - 1; index >= 0; index--)
            if (_messages[index].Role == role)
            {
                message = _messages[index];
                return true;
            }

        message = null;
        return false;
    }
}