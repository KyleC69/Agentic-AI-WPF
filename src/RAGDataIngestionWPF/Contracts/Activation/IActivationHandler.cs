// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IActivationHandler.cs
// Author: Kyle L. Crowder
// Build Num: 212929



namespace AgenticAIWPF.Contracts.Activation;





public interface IActivationHandler
{
    bool CanHandle();


    Task HandleAsync();
}