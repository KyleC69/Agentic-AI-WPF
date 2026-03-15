// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         AIContextHistoryInjector.cs
// Author: Kyle L. Crowder
// Build Num: 090952



using Microsoft.Agents.AI;




namespace DataIngestionLib.Services.ContextInjectors;





public sealed class AIContextHistoryInjector : AIContextProvider
{





    /// <inheritdoc />
    protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {

        return base.InvokedCoreAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask<AIContext> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {

        return base.InvokingCoreAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken cancellationToken = new CancellationToken())
    {

        return base.ProvideAIContextAsync(context, cancellationToken);
    }








    /// <inheritdoc />
    protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = new CancellationToken())
    {

        return base.StoreAIContextAsync(context, cancellationToken);
    }
}