// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         IIdentityCacheService.cs
// Author: Kyle L. Crowder
// Build Num: 233150



namespace RAGDataIngestionWPF.Core.Contracts.Services;





public interface IIdentityCacheService
{

    byte[] ReadMsalToken();


    void SaveMsalToken(byte[] token);
}