// Copyright (c) Your Organization. All rights reserved.



using System.Text.Json;

using Microsoft.Agents.Builder;
using Microsoft.Agents.Core.Models;
// ITurnContext



// IActivity, ChannelAccount, ConversationAccount

namespace DataIngestionLib.Providers.ChatHistory;

/// <summary>
/// Snapshot of all built-in activity properties that should be persisted alongside a
/// chat message.  Extracted from <see cref="ITurnContext"/> at the point the message is
/// captured, so that the exact state of the incoming activity is faithfully recorded.
/// </summary>
public sealed class ActivityMetadata
{
    // ── Core activity identity ────────────────────────────────────────────────
    public string? ActivityId { get; init; }
    public string? ActivityType { get; init; }
    public DateTimeOffset? Timestamp { get; init; }
    public string? ServiceUrl { get; init; }

    // ── Sender (Activity.From) ────────────────────────────────────────────────
    public string? FromId { get; init; }
    public string? FromName { get; init; }
    public string? FromRole { get; init; }
    public string? FromAadObjectId { get; init; }

    // ── Recipient / Agent (Activity.Recipient) ────────────────────────────────
    public string? RecipientId { get; init; }
    public string? RecipientName { get; init; }
    public string? RecipientRole { get; init; }

    // ── Thread / reply chain ──────────────────────────────────────────────────
    public string? ReplyToId { get; init; }

    // ── Formatting ────────────────────────────────────────────────────────────
    public string? Locale { get; init; }
    public string? TextFormat { get; init; }
    public string? InputHint { get; init; }

    // ── Conversation context (Activity.Conversation) ──────────────────────────
    public string? ConversationName { get; init; }
    public string? ConversationType { get; init; }
    public bool? IsGroupConversation { get; init; }

    // ── Overflow ──────────────────────────────────────────────────────────────

    /// <summary>
    /// JSON-serialised form of <c>Activity.Entities</c>.  Preserves mentions, geo-coordinates,
    /// Teams entity data, etc. without requiring a dedicated schema column per entity type.
    /// </summary>
    public string? EntitiesJson { get; init; }

    // ── Factory ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Extracts all built-in properties from the current <see cref="ITurnContext"/> and
    /// returns an immutable <see cref="ActivityMetadata"/> snapshot.
    /// </summary>
    /// <param name="turnContext">The active turn context.</param>
    /// <returns>A fully-populated metadata snapshot.</returns>
    public static ActivityMetadata FromTurnContext(ITurnContext turnContext)
    {
        ArgumentNullException.ThrowIfNull(turnContext);

        IActivity activity = turnContext.Activity;
        ChannelAccount? from = activity.From;
        ChannelAccount? recipient = activity.Recipient;
        ConversationAccount? conversation = activity.Conversation;

        string? entitiesJson = null;
        if (activity.Entities is { Count: > 0 })
        {
            try
            {
                entitiesJson = JsonSerializer.Serialize(activity.Entities);
            }
            catch
            {
                // Serialization of channel-specific entities should never crash the turn.
            }
        }

        return new ActivityMetadata
        {
            // Core
            ActivityId        = activity.Id,
            ActivityType      = activity.Type,
            Timestamp         = activity.Timestamp,
            ServiceUrl        = activity.ServiceUrl,

            // Sender
            FromId            = from?.Id,
            FromName          = from?.Name,
            FromRole          = from?.Role,
            FromAadObjectId   = from?.AadObjectId,

            // Recipient
            RecipientId       = recipient?.Id,
            RecipientName     = recipient?.Name,
            RecipientRole     = recipient?.Role,

            // Thread
            ReplyToId         = activity.ReplyToId,

            // Formatting
            Locale            = activity.Locale,
            TextFormat        = activity.TextFormat,
            InputHint         = activity.InputHint,

            // Conversation
            ConversationName  = conversation?.Name,
            ConversationType  = conversation?.ConversationType,
            IsGroupConversation = conversation?.IsGroup,

            // Overflow
            EntitiesJson      = entitiesJson,
        };
    }
}
