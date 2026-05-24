// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Core
// File:         IIdentityCacheService.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



namespace AgenticAIWPF.Core.Contracts.Services;





public interface IIdentityCacheService
{

    byte[] ReadMsalToken();


    void SaveMsalToken(byte[] token);
}