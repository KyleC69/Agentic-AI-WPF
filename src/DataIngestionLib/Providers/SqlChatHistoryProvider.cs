// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         SqlChatHistoryProvider.cs
// Author: Kyle L. Crowder
// Build Num: 051933



using DataIngestionLib.Contracts;
using DataIngestionLib.History.HistoryModels;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public class SqlChatHistoryProvider : ChatHistoryProvider
{
    private readonly IAppSettings _appSettings;
    private readonly ILogger<SqlChatHistoryProvider> _logger;
    private PersistenceKeys _persistenceKeys;

    //Testing filters to bypass defaults and allow everythting through.
    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> providerInputFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);
    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> storeInputRequestFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);
    private static readonly Func<IEnumerable<ChatMessage>, IEnumerable<ChatMessage>> storeInputResponseFilter = msgs => msgs.Where(msg => msg.Text != string.Empty);








    public SqlChatHistoryProvider(IAppSettings appSettings, ILogger<SqlChatHistoryProvider> logger) : base(providerInputFilter, storeInputRequestFilter, storeInputResponseFilter)
    {

        _appSettings = appSettings;
        _logger = logger;
        PersistenceKeys.ApplicationId = _appSettings.ApplicationId;
        PersistenceKeys.UserId = _appSettings.UserId;
    }








    /// <inheritdoc />
    protected override ValueTask<IEnumerable<ChatMessage>> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return base.InvokingCoreAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {

        var msgs = context.RequestMessages;
        InsertUserMessages(msgs, PersistenceKeys.ApplicationId, PersistenceKeys.SessionId, PersistenceKeys.UserId, PersistenceKeys.ConversationId, PersistenceKeys.MessageId);

        return base.ProvideChatHistoryAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        var msgs = context.RequestMessages;
        ChatHistoryMessage message = new ChatHistoryMessage
        {
                MessageId = Guid.TryParse(context.ResponseMessages.LastOrDefault()?.MessageId, out Guid messageId) ? messageId : Guid.NewGuid(),
                ConversationId = "conv1"
        };


        return base.StoreChatHistoryAsync(context, cancellationToken);
    }








    private void InsertUserMessages(IEnumerable<ChatMessage> msgs, string ApplicationId, string SessionId, string UserId, string ConversationId, string MessageId)
    {
        ChatHistoryMessage message = new ChatHistoryMessage
        {
                MessageId = Guid.NewGuid(),
                ConversationId = "conv1"
        };



    }
}





public class PersistenceKeys
{

    public static string ApplicationId { get; set; }

    public static string ConversationId { get; set; }

    public static string MessageId { get; set; }

    public static string SessionId { get; set; }

    public static string UserId { get; set; }
}