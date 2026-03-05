namespace DataIngestionLib.Models;

public sealed record ChatSessionState
{
    public IReadOnlyList<ChatMessage> History { get; init; } = [];

    public IReadOnlyList<ChatMessage> ContextWindow { get; init; } = [];

    public int ContextTokenCount { get; init; }
}
