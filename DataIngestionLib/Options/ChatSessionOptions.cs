namespace DataIngestionLib.Options;

public sealed class ChatSessionOptions
{
    public string ConfigurationsFolder { get; set; } = string.Empty;

    public string ChatSessionFileName { get; set; } = "ChatSession.json";

    public int MaxContextTokens { get; set; } = 120000;
}
