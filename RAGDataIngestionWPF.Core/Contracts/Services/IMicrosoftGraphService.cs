// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF.Core
//  File:         IMicrosoftGraphService.cs
//   Author: Kyle L. Crowder



using RAGDataIngestionWPF.Core.Models;




namespace RAGDataIngestionWPF.Core.Contracts.Services;





public interface IMicrosoftGraphService
{
    Task<User> GetUserInfoAsync(string accessToken);


    Task<string> GetUserPhoto(string accessToken);
}