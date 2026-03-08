using System.Text.Json.Serialization;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.Shared.Diagnostics;

using State = Microsoft.Agents.AI.ChatHistoryMemoryProvider.State;


namespace DataIngestionLib.Services;



/// <summary>
/// This class shows how to implement a SQL-backed chat history provider. Why store chat history in SQL?
/// It simplifies the life cycle hooks, all you need to do is get the last session id used by this agent in this application.
/// Each agent has an Id each agent has a session id and an application can have an id. Why all the Id's?
/// Because you can have multiple agents in the same application and each agent can have multiple sessions.
/// This allows you to easily query the chat history for a specific agent and session, and also allows you to easily clean up
/// old chat history by deleting or deactivating rows from the database that are older than a certain date or that belong to a specific session or agent.
/// How about creating a semantic chat reducer that ensures you are using the most relevant information not only by date but also relevance, that is power!
/// If this is in an application in an enterprise and company policy to save all history for security and safety reasons, you have an onsite SQL server that stores it
/// for each application in the Enterprise, Each agent multiple sessions storage needs exponentially growing. Sound unnecessary?
/// Now when you pair it with an AIContextProvider. Now you can search the entire history of interactions for a specific agent and session to find relevant information amd
/// inject it into the current conversation. Now you have a powerful way to provide context to the agent imaging 100 users all providing context sources
/// that can be used at any time to provide relevant information to the agent. This is a powerful way to provide context to the agent and can help improve the quality of the responses provided by the agent.
/// Add vector search capabilities to the database, and now you can grab info that is hyper-relevant to the current conversation and inject it into the conversation to provide even more context to the agent.
/// </summary>
public class SqlChatHistoryProvider : ChatHistoryProvider
{
    private string _connString;
    private readonly ProviderSessionState<State> _sessionState;
    private string _applicationId;
    public SqlChatHistoryProvider(Func<AgentSession?, State>? stateInitializer = null, string? stateKey = null)
    {
        // Initialize your SQL connection here using the connection string from the environment variable
        // You can use any SQL library you prefer, such as Dapper, Entity Framework, ADO.NET, etc.
        // For example, using Dapper:
        // _connection = new SqlConnection(CONNSTRING);

        _connString = Environment.GetEnvironmentVariable("CHAT_HISTORY");
        
        this._sessionState = new ProviderSessionState<State>(
                stateInitializer ?? (_ => new State()), stateKey ?? this.GetType().Name);
        
    }

    public string StateKey => this._sessionState.StateKey;

    protected override ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = default) =>
            // return all messages in the session state
            new(this._sessionState.GetOrInitializeState(context.Session).Messages);

    protected override ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        var state = this._sessionState.GetOrInitializeState(context.Session);

        // Add both request and response messages to the session state.
        var allNewMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []);
        state.Messages.AddRange(allNewMessages);

        this._sessionState.SaveState(context.Session, state);

        return default;
    }

    public sealed class State
    {
        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = [];
    }




}


/// <summary>
/// The data structure used to store chat history items in the vector store.
/// </summary>
internal sealed class ChatHistoryItem
{
    public string ApplicationId { get; set; }
    public string UserId { get; set; }
    public string AgentId { get; set; }
    

    [VectorStoreKey]
    public string? Key { get; set; }

    [VectorStoreData]
    public string? SessionId { get; set; }

    [VectorStoreData]
    public DateTimeOffset? Timestamp { get; set; }

    [VectorStoreData]
    public string? SerializedMessage { get; set; }

    [VectorStoreData]
    public string? MessageText { get; set; }
}