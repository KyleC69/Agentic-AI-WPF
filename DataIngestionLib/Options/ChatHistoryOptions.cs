namespace DataIngestionLib.Options;

public sealed class ChatHistoryOptions
{
    public const string ConfigurationSectionName = "ChatHistory";

    public string ConnectionString { get; set; } = "Server=(localdb)\\MSSQLLocalDB;Database=RAGDataIngestionChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";

    public int MaxContextMessages { get; set; } = 16;

    public int? MaxContextTokens { get; set; } = 120000;

    public bool EnableSummarization { get; set; }
}
