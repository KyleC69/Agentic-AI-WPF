// 2026/03/10
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         IApplicationIdService.cs
//   Author: Kyle L. Crowder



namespace RAGDataIngestionWPF.Contracts.Services;





public interface IApplicationIdService
{
    Guid GetApplicationId();


    Guid RenewApplicationId();
}