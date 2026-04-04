// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Contracts;





public interface ISqlChatHistoryProvider
{
    /// <summary>
    ///     Gets the set of keys used to store the provider state in the
    ///     <see cref="P:Microsoft.Agents.AI.AgentSession.StateBag" />.
    /// </summary>
    /// <remarks>
    ///     The default value is a single-element set containing the name of the concrete type (e.g.
    ///     <c>"InMemoryChatHistoryProvider"</c>).
    ///     Implementations may override this to provide custom keys, for example when multiple
    ///     instances of the same provider type are used in the same session, or when a provider
    ///     stores state under more than one key.
    /// </remarks>
    IReadOnlyList<string> StateKeys { get; }


    ValueTask<string?> GetLatestConversationIdAsync(CancellationToken cancellationToken = default);








    /// <summary>
    ///     Asynchronously retrieves a collection of chat messages for a specified conversation.
    /// </summary>
    /// <param name="conversationId">
    ///     The unique identifier of the conversation for which chat messages are to be retrieved.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an enumerable collection of
    ///     <see cref="ChatMessage" /> objects associated with the specified conversation.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="conversationId" /> is <c>null</c>.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    ///     Thrown if the operation is canceled via the <paramref name="cancellationToken" />.
    /// </exception>
    ValueTask<IEnumerable<ChatMessage>?> GetMessagesAsync(string conversationId, CancellationToken cancellationToken = default);








    ValueTask<IEnumerable<ChatMessage>> InvokingAsync(ChatHistoryProvider.InvokingContext context, CancellationToken cancellationToken);


    ValueTask InvokedAsync(ChatHistoryProvider.InvokedContext context, CancellationToken cancellationToken);


    object? GetService(Type serviceType, object? serviceKey);



}