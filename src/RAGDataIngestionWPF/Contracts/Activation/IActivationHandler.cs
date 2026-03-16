// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IActivationHandler.cs
// Author: Kyle L. Crowder
// Build Num: 051856



namespace RAGDataIngestionWPF.Contracts.Activation;





public interface IActivationHandler
{
    bool CanHandle();


    Task HandleAsync();
}