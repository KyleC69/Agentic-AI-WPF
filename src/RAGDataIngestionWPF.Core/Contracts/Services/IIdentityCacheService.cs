// Build Date: 2026/03/16
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         IIdentityCacheService.cs
// Author: Kyle L. Crowder
// Build Num: 051916



namespace RAGDataIngestionWPF.Core.Contracts.Services;





public interface IIdentityCacheService
{

    byte[] ReadMsalToken();


    void SaveMsalToken(byte[] token);
}