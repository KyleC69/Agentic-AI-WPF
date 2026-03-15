// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IActivationHandler.cs
// Author: Kyle L. Crowder
// Build Num: 091008



namespace RAGDataIngestionWPF.Contracts.Activation;





public interface IActivationHandler
{
    bool CanHandle();


    Task HandleAsync();
}