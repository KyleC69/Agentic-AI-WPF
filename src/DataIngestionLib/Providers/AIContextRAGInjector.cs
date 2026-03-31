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



using CommunityToolkit.Diagnostics;

using DataIngestionLib.Services;

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;




namespace DataIngestionLib.Providers;





public sealed class AIContextRAGInjector : MessageAIContextProvider
{
    private readonly RagDataService _ragData;








    public AIContextRAGInjector(RagDataService ragData)
    {
        Guard.IsNotNull(ragData);
        _ragData = ragData;

    }








    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ChatMessage search = context.RequestMessages.Last();

        List<ChatMessage> results = await _ragData.GetRagDataEntries(search.Text);


        return results;



    }








    /// <summary>
    ///     Stores the AI context asynchronously after the invocation of a specific operation.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="InvokedContext" /> containing details about the invoked operation and its associated data.
    /// </param>
    /// <param name="cancellationToken">
    ///     A <see cref="CancellationToken" /> that can be used to observe cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="ValueTask" /> representing the asynchronous operation.
    /// </returns>
    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        return ValueTask.CompletedTask;
    }
}