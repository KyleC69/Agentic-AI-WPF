// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IActivationHandler.cs
// Author: Kyle L. Crowder
// Build Num: 194528



namespace AgenticAIWPF.Contracts.Activation;





public interface IActivationHandler
{
    bool CanHandle();


    Task HandleAsync();
}