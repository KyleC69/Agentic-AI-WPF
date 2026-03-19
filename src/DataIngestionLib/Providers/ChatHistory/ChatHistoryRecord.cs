// Copyright (c) Your Organization. All rights reserved.

namespace DataIngestionLib.Providers.ChatHistory;

/// <summary>
/// Database row model for a single chat message.
///
/// Suggested SQL Server DDL:
/// <code>
/// CREATE TABLE ChatHistoryMessages (
///     RowId            BIGINT IDENTITY(1,1) PRIMARY KEY,
///
///     -- Enterprise composite key ----------------------------------------
///     ApplicationId    UNIQUEIDENTIFIER NOT NULL,
///     UserId           NVARCHAR(500)    NOT NULL,
///     AgentId          NVARCHAR(500)    NOT NULL,
///     ConversationId   NVARCHAR(500)    NOT NULL,
///     ChannelId        NVARCHAR(200)    NOT NULL,
///     TenantId         NVARCHAR(200)        NULL,
///
///     -- Message body -------------------------------------------------------
///     Role             NVARCHAR(50)     NOT NULL,   -- user | assistant | system | tool
///     Content          NVARCHAR(MAX)    NOT NULL,
///     AuthorName       NVARCHAR(500)        NULL,
///     ModelId          NVARCHAR(500)        NULL,   -- model that generated assistant turn
///     SequenceNumber   INT              NOT NULL,
///     CreatedAt        DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
///
///     -- Built-in activity properties (propagated from IActivity) -----------
///     ActivityId       NVARCHAR(500)        NULL,   -- Activity.Id
///     ActivityType     NVARCHAR(100)        NULL,   -- Activity.Type
///     FromId           NVARCHAR(500)        NULL,   -- Activity.From.Id
///     FromName         NVARCHAR(500)        NULL,   -- Activity.From.Name
///     FromRole         NVARCHAR(100)        NULL,   -- Activity.From.Role
///     FromAadObjectId  NVARCHAR(500)        NULL,   -- Activity.From.AadObjectId
///     RecipientId      NVARCHAR(500)        NULL,   -- Activity.Recipient.Id
///     RecipientName    NVARCHAR(500)        NULL,   -- Activity.Recipient.Name
///     RecipientRole    NVARCHAR(100)        NULL,   -- Activity.Recipient.Role
///     ReplyToId        NVARCHAR(500)        NULL,   -- Activity.ReplyToId
///     Locale           NVARCHAR(50)         NULL,   -- Activity.Locale
///     TextFormat       NVARCHAR(50)         NULL,   -- Activity.TextFormat (plain|markdown|xml)
///     InputHint        NVARCHAR(50)         NULL,   -- Activity.InputHint
///     ServiceUrl       NVARCHAR(2048)       NULL,   -- Activity.ServiceUrl
///     ActivityTimestamp DATETIMEOFFSET      NULL,   -- Activity.Timestamp
///     ConversationName NVARCHAR(500)        NULL,   -- Activity.Conversation.Name
///     ConversationType NVARCHAR(100)        NULL,   -- Activity.Conversation.ConversationType
///     IsGroupConversation BIT              NULL,    -- Activity.Conversation.IsGroup
///
///     -- Overflow for complex/channel-specific properties -------------------
///     EntitiesJson     NVARCHAR(MAX)        NULL,   -- Activity.Entities (serialised)
///     MetadataJson     NVARCHAR(MAX)        NULL,   -- ChatMessageContent.Metadata (serialised)
///
///     -- Indexes -------------------------------------------------------------
///     INDEX IX_ChatHistory_Key (ApplicationId, UserId, AgentId, ConversationId, SequenceNumber),
///     INDEX IX_ChatHistory_UserAgent (ApplicationId, UserId, AgentId),
///     INDEX IX_ChatHistory_Agent (ApplicationId, AgentId)
/// );
/// </code>
/// </summary>
public sealed class ChatHistoryRecord
{
    // ── Row identity ──────────────────────────────────────────────────────────
    public long RowId { get; set; }

    // ── Enterprise composite key ──────────────────────────────────────────────
    public Guid ApplicationId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AgentId { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string? TenantId { get; set; }

    // ── Message body ──────────────────────────────────────────────────────────
    public string Role { get; set; } = string.Empty;       // "user" | "assistant" | "system" | "tool"
    public string Content { get; set; } = string.Empty;
    public string? AuthorName { get; set; }                // Display name of the message author
    public string? ModelId { get; set; }                   // LLM model that produced an assistant turn
    public int SequenceNumber { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // ── Built-in activity properties ──────────────────────────────────────────
    // All fields below are propagated directly from the IActivity that arrived
    // during the turn in which this message was produced.

    /// <summary>Activity.Id — framework-assigned unique message identifier.</summary>
    public string? ActivityId { get; set; }

    /// <summary>Activity.Type — typically "message" for chat, but also "event", "invoke", etc.</summary>
    public string? ActivityType { get; set; }

    // Sender (Activity.From)
    public string? FromId { get; set; }
    public string? FromName { get; set; }
    public string? FromRole { get; set; }                  // "user" | "bot" | "skill"
    public string? FromAadObjectId { get; set; }           // AAD Object ID when available

    // Agent / Bot recipient (Activity.Recipient)
    public string? RecipientId { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientRole { get; set; }

    /// <summary>Activity.ReplyToId — the ID of the activity this message is replying to.</summary>
    public string? ReplyToId { get; set; }

    /// <summary>Activity.Locale — IETF locale tag (e.g. "en-US").</summary>
    public string? Locale { get; set; }

    /// <summary>Activity.TextFormat — "plain", "markdown", or "xml".</summary>
    public string? TextFormat { get; set; }

    /// <summary>Activity.InputHint — "acceptingInput", "ignoringInput", "expectingInput".</summary>
    public string? InputHint { get; set; }

    /// <summary>Activity.ServiceUrl — used for proactive messaging; not always present.</summary>
    public string? ServiceUrl { get; set; }

    /// <summary>Activity.Timestamp — wall-clock time at the channel.</summary>
    public DateTimeOffset? ActivityTimestamp { get; set; }

    // Conversation metadata (Activity.Conversation)
    public string? ConversationName { get; set; }
    public string? ConversationType { get; set; }          // e.g. "personal", "channel", "groupChat"
    public bool? IsGroupConversation { get; set; }

    // ── Overflow serialised fields ────────────────────────────────────────────

    /// <summary>
    /// JSON array of Activity.Entities (mentions, geo, place etc.).
    /// Serialise with <c>System.Text.Json.JsonSerializer</c>.
    /// </summary>
    public string? EntitiesJson { get; set; }

    /// <summary>
    /// JSON object of ChatMessageContent.Metadata (SK-level metadata such as token usage,
    /// finish reason, logprobs, etc.).
    /// </summary>
    public string? MetadataJson { get; set; }
}
