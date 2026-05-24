// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IActivationHandler.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



namespace AgenticAIWPF.Contracts.Activation;





public interface IActivationHandler
{
    bool CanHandle();


    Task HandleAsync();
}