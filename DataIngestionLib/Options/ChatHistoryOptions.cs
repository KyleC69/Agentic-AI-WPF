// 2026/03/08
//  Solution: RAGDataIngestionWPF
//  Project:   DataIngestionLib
//  File:         ChatHistoryOptions.cs
//   Author: Kyle L. Crowder



namespace DataIngestionLib.Options;





public sealed class ChatHistoryOptions
{
    public const string ConfigurationSectionName = "ChatHistory";

    public string ConnectionString { get; set; } = "Server=(localdb)\\MSSQLLocalDB;Database=RAGDataIngestionChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";

    public string EmbeddingsModelName { get; set; } = "mxbai-embed-large-v1:latest";
    public string ChatModelName { get; set; } = "gpt-oss:latest";

    /// <summary>
    /// Gets or sets a value indicating whether summarization of pruned chat history messages is enabled.
    /// </summary>
    /// <remarks>
    /// When enabled, pruned messages from the chat history are summarized and the summary is added back to the chat history.
    /// This helps retain the context of removed messages while adhering to the constraints of maximum messages or tokens.
    /// </remarks>
    public bool EnableSummarization { get; set; } = false; //is it practical to summarize in real-time on enterprise environment that retain full history?

    
    /// <summary>
    /// Gets or sets the maximum number of context messages  that get injected in context.
    /// </summary>
    /// <remarks>This property determines the limit on the number of context messages retained in memory.
    /// Exceeding this limit may result in older messages being discarded.</remarks>
    public int MaxContextMessages { get; set; } = 16;
    /// <summary>
    /// Sets the size of the context window before context injections.
    /// </summary>
    public int? MaxContextTokens { get; set; } = 130000;
    /// <summary>
    /// Enabled the use of past chat history message as part of the cotext. Can be extremely useful for enterprise scenarios where the entire history of the conversation needs to be retained and used as part of the context for future interactions. When enabled, past messages from the chat history are
    /// Ideal Scenario : Call centers or support systems using LLM's to assist agents in providing better customer service by retaining the entire history of customer interactions, allowing for more informed responses and personalized assistance.
    /// This is not the same as RAG Knowledge, which may consiste of in-house documents or very specific domain contents like repair manuals, product catalogs, etc... 
    /// </summary>
    public bool SemanticHistoryEnabled { get; set; } = true;
    /// <summary>
    /// can be used for documents, manuals, websites etc... that are relevant to the conversation but not part of the conversation history. For example, in a customer support scenario,
    /// RAG knowledge could include product manuals, troubleshooting guides, or FAQs that are relevant to the customer's issue but not part of the actual conversation history. When enabled, relevant external knowledge is retrieved and injected into the context to enhance the model's understanding and response generation.
    /// </summary>
    public bool RAGKnowledgeEnabled { get; set; } = true;
    /// <summary>
    /// Max amount of chat messages to use in the context.
    /// </summary>
    public int MaxSemanticMessages { get; set; } = 8;
    


    public ChatHistoryMode PruneMode { get; set; } = ChatHistoryMode.MessageCount;
    public ChatHistoryRetentionPolicy RetentionPolicy { get; set; } = ChatHistoryRetentionPolicy.TimeBased;
}
public enum ChatHistoryMode
{
    /// <summary>
    ///     Maintain a fixed sized context window dropping off the oldest messages based on the total token count of the context, ensuring it does not exceed the specified
    ///     maximum.
    /// </summary>
    SlidingTokenWindow,
    /// <summary>
    ///     Prune messages based on the total number of messages in the context, ensuring it does not exceed the
    ///     specified maximum.
    /// </summary>
    MessageCount,
    /// <summary>
    ///  Limits the size of the context by pruning messages based on the total token count, ensuring it does not exceed the specified maximum.
    /// </summary>
    TokenCount,
    /// <summary>
    /// Represents a sliding window of messages, allowing for efficient management and retrieval of recent messages
    /// within a specified Message count.
    /// </summary>
    /// <remarks>This class is useful for scenarios where you need to keep track of messages that arrive over
    /// time, such as logging or event handling. The sliding window automatically discards messages that fall outside
    /// the defined time range, ensuring that only relevant messages are retained.</remarks>
    SlidingMessageWindow

}
public enum ChatHistoryRetentionPolicy
{
    /// <summary>
    ///     Retain the most recent messages in the chat history, pruning older messages by default when limits are exceeded.
    /// </summary>
    TimeBased,
    /// <summary>
    ///     Retain the oldest messages in the chat history, pruning newer messages first when limits are exceeded.
    /// </summary>
    KeepOldest
}