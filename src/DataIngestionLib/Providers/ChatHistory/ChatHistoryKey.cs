// Copyright (c) Your Organization. All rights reserved.

namespace DataIngestionLib.Providers.ChatHistory;

/// <summary>
/// Composite key that uniquely identifies a chat history session across enterprise dimensions.
/// Provides granular isolation by application, user, agent, conversation, and channel.
/// </summary>
/// <param name="ApplicationId">
///   Enterprise application identifier. Allows a single SQL database to serve multiple
///   applications without data bleed between them.
/// </param>
/// <param name="UserId">
///   The end-user identifier, drawn from <c>Activity.From.Id</c>.  In Azure Bot Service
///   this is a channel-scoped identity (e.g. AAD object ID or Teams user MRI).
/// </param>
/// <param name="AgentId">
///   Logical agent identifier chosen by the implementing team (e.g. "support-agent-v2").
///   Multiple agents can share the same database whilst keeping histories separate.
/// </param>
/// <param name="ConversationId">
///   The framework-assigned conversation identifier from <c>Activity.Conversation.Id</c>.
///   Scopes history to a single conversation thread.
/// </param>
/// <param name="ChannelId">
///   The channel that originated the conversation (e.g. "msteams", "webchat", "directline").
///   Drawn from <c>Activity.ChannelId</c>.
/// </param>
/// <param name="TenantId">
///   Optional Azure AD tenant identifier from <c>Activity.Conversation.TenantId</c>.
///   Useful for multi-tenant SaaS scenarios.
/// </param>
public sealed record ChatHistoryKey(
    Guid ApplicationId,
    string UserId,
    string AgentId,
    string ConversationId,
    string ChannelId,
    string? TenantId = null)
{
    /// <summary>
    /// Returns a string representation suitable for logging and diagnostics.
    /// Does NOT include the full ConversationId to guard against accidental leakage into
    /// log aggregators.
    /// </summary>
    public override string ToString() =>
        $"App={ApplicationId:D}, User={Mask(UserId)}, Agent={AgentId}, " +
        $"Conv={Mask(ConversationId)}, Channel={ChannelId}";

    private static string Mask(string value) =>
        value.Length <= 6 ? "***" : $"{value[..3]}***{value[^3..]}";
}
