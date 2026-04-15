// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Core
// File:         IIdentityCacheService.cs
// Author: Kyle L. Crowder
// Build Num: 194525



namespace AgenticAIWPF.Core.Contracts.Services;





public interface IIdentityCacheService
{

    byte[] ReadMsalToken();


    void SaveMsalToken(byte[] token);
}