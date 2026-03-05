namespace DataIngestionLib.Models;

public sealed record ChatMessage
{
    public ChatMessageRole Role { get; init; }

    public string Content { get; init; } = string.Empty;

    public string FormattedContent { get; init; } = string.Empty;

    public DateTimeOffset TimestampUtc { get; init; }

    public int TokenCount { get; init; }

    public bool IsUser => Role == ChatMessageRole.User;
}
