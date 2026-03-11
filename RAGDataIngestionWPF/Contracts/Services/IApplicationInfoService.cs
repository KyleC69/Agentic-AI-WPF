// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         IApplicationInfoService.cs
//   Author: Kyle L. Crowder



namespace RAGDataIngestionWPF.Contracts.Services;





public interface IApplicationInfoService
{
    Version GetVersion();
}