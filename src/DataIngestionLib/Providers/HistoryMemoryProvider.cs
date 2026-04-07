// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         HistoryMemoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 212911



using DataIngestionLib.Contracts;
using DataIngestionLib.Services;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;




namespace DataIngestionLib.Providers;





public sealed class HistoryMemoryProvider : ChatHistoryProvider, IChatHistoryMemoryProvider
{

    private readonly Dictionary<string, List<ChatMessage>> _conversationWindows = new(StringComparer.OrdinalIgnoreCase);
    private readonly IHistoryIdentityService _historyIdentityService;
    private readonly Lock _windowLock = new();
    private readonly int _windowSize;
    private const int DEFAULT_WINDOW_SIZE = 40;








    public HistoryMemoryProvider(IHistoryIdentityService historyIdentityService, int? windowSize = null) : base(messages => messages, messages => messages, messages => messages)
    {
        ArgumentNullException.ThrowIfNull(historyIdentityService);

        _historyIdentityService = historyIdentityService;
        _windowSize = windowSize.GetValueOrDefault(DEFAULT_WINDOW_SIZE);
        if (_windowSize <= 0)
        {
            _windowSize = DEFAULT_WINDOW_SIZE;
        }
    }








    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context is null)
        {
            return [];
        }

        var conversationId = context.Session?.StateBag?.GetValue<string>("ConversationId") ?? _historyIdentityService.Current.ConversationId;
        var window = await BuildContextMessagesForRequestAsync(conversationId, context.RequestMessages ?? [], cancellationToken).ConfigureAwait(false);

        return window.Select(message => message.WithAgentRequestMessageSource(AgentRequestMessageSourceType.ChatHistory, nameof(HistoryMemoryProvider))).ToList();
    }








    protected override async ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context is null)
        {
            return;
        }

        HistoryIdentity identity = _historyIdentityService.Current;
        var conversationId = context.Session?.StateBag?.GetValue<string>("ConversationId") ?? identity.ConversationId;
        var requestMessages = context.RequestMessages ?? [];
        var responseMessages = context.ResponseMessages ?? [];

        await StoreMessagesInternalAsync(conversationId, requestMessages, responseMessages, cancellationToken).ConfigureAwait(false);
    }








    public override IReadOnlyList<string> StateKeys
    {
        get { return []; }
    }








    public ValueTask<IEnumerable<ChatMessage>> BuildContextMessagesAsync(string conversationId, ChatHistory currentRequestMessages, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(currentRequestMessages);

        var effectiveConversationId = ResolveConversationId(conversationId);
        var requestFingerprints = BuildFingerprintSet(ConvertKernelHistoryToChatMessages(currentRequestMessages));

        List<ChatMessage> snapshot;
        lock (_windowLock)
        {
            if (!_conversationWindows.TryGetValue(effectiveConversationId, out var allMessages) || allMessages.Count == 0)
            {
                return ValueTask.FromResult<IEnumerable<ChatMessage>>([]);
            }

            snapshot = allMessages.TakeLast(_windowSize).Where(ShouldIncludeInContext).Where(message => !requestFingerprints.Contains(CreateFingerprint(message))).ToList();
        }

        return ValueTask.FromResult<IEnumerable<ChatMessage>>(snapshot);
    }








    public ValueTask StoreMessagesAsync(string conversationId, string agentId, string userId, string applicationId, ChatHistory requestMessages, ChatHistory responseMessages, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(requestMessages);
        ArgumentNullException.ThrowIfNull(responseMessages);

        var effectiveConversationId = ResolveConversationId(conversationId);
        var persistableMessages = ConvertKernelHistoryToChatMessages(requestMessages).Concat(ConvertKernelHistoryToChatMessages(responseMessages)).Where(ShouldPersistMessage).ToList();

        if (persistableMessages.Count == 0)
        {
            return ValueTask.CompletedTask;
        }

        lock (_windowLock)
        {
            if (!_conversationWindows.TryGetValue(effectiveConversationId, out var window))
            {
                window = [];
                _conversationWindows[effectiveConversationId] = window;
            }

            window.AddRange(persistableMessages);
            while (window.Count > _windowSize) window.RemoveAt(0);
        }

        return ValueTask.CompletedTask;
    }








    private async ValueTask<IEnumerable<ChatMessage>> BuildContextMessagesForRequestAsync(string conversationId, IEnumerable<ChatMessage> requestMessages, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requestFingerprints = BuildFingerprintSet(requestMessages);
        var effectiveConversationId = ResolveConversationId(conversationId);

        List<ChatMessage> snapshot;
        lock (_windowLock)
        {
            if (!_conversationWindows.TryGetValue(effectiveConversationId, out var allMessages) || allMessages.Count == 0)
            {
                return [];
            }

            snapshot = allMessages.TakeLast(_windowSize).Where(ShouldIncludeInContext).Where(message => !requestFingerprints.Contains(CreateFingerprint(message))).ToList();
        }

        return await ValueTask.FromResult<IEnumerable<ChatMessage>>(snapshot).ConfigureAwait(false);
    }








    private static HashSet<string> BuildFingerprintSet(IEnumerable<ChatMessage> messages)
    {
        HashSet<string> fingerprints = new(StringComparer.Ordinal);
        foreach (ChatMessage message in messages) _ = fingerprints.Add(CreateFingerprint(message));

        return fingerprints;
    }








    private static IEnumerable<ChatMessage> ConvertKernelHistoryToChatMessages(ChatHistory history)
    {
        foreach (ChatMessageContent entry in history)
        {
            var content = entry.Content ?? string.Empty;
            if (string.IsNullOrWhiteSpace(content))
            {
                continue;
            }

            yield return new ChatMessage(MapRole(entry.Role), content);
        }
    }








    private static string CreateFingerprint(ChatMessage message)
    {
        var role = message.Role.Value;
        var messageId = message.MessageId ?? string.Empty;
        var text = message.Text ?? string.Empty;
        return $"{role}|{messageId}|{text}";
    }








    private static bool HasExplicitSourceType(ChatMessage message, AgentRequestMessageSourceType sourceType)
    {
        return message.AdditionalProperties is null ? false : message.AdditionalProperties.TryGetValue(AgentRequestMessageSourceAttribution.AdditionalPropertiesKey, out var value) && value is AgentRequestMessageSourceAttribution attribution && attribution.SourceType == sourceType;
    }








    private static ChatRole MapRole(AuthorRole role)
    {
        return role == AuthorRole.System ? ChatRole.System : role == AuthorRole.Assistant ? ChatRole.Assistant : role == AuthorRole.Tool ? ChatRole.Tool : ChatRole.User;
    }








    private string ResolveConversationId(string? conversationId)
    {
        return !string.IsNullOrWhiteSpace(conversationId) ? conversationId : _historyIdentityService.Current.ConversationId;
    }








    private static bool ShouldIncludeInContext(ChatMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Text))
        {
            return false;
        }

        // Tool results are available only in the current turn and should not be replayed.
        return message.Role != ChatRole.Tool;
    }








    private static bool ShouldPersistMessage(ChatMessage message)
    {
        return !ShouldIncludeInContext(message) ? false : !HasExplicitSourceType(message, AgentRequestMessageSourceType.ChatHistory);
    }








    private ValueTask StoreMessagesInternalAsync(string conversationId, IEnumerable<ChatMessage> requestMessages, IEnumerable<ChatMessage> responseMessages, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var effectiveConversationId = ResolveConversationId(conversationId);
        var persistableMessages = requestMessages.Concat(responseMessages).Where(ShouldPersistMessage).ToList();

        if (persistableMessages.Count == 0)
        {
            return ValueTask.CompletedTask;
        }

        lock (_windowLock)
        {
            if (!_conversationWindows.TryGetValue(effectiveConversationId, out var window))
            {
                window = [];
                _conversationWindows[effectiveConversationId] = window;
            }

            window.AddRange(persistableMessages);
            while (window.Count > _windowSize) window.RemoveAt(0);
        }

        return ValueTask.CompletedTask;
    }
}