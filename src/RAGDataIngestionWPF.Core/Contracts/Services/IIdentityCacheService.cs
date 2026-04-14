// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Core
// File:         IIdentityCacheService.cs
// Author: Kyle L. Crowder
// Build Num: 212926



namespace AgenticAIWPF.Core.Contracts.Services;





public interface IIdentityCacheService
{

    byte[] ReadMsalToken();


    void SaveMsalToken(byte[] token);
}