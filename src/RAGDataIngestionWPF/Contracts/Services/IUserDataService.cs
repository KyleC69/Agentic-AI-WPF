// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IUserDataService.cs
// Author: Kyle L. Crowder
// Build Num: 212930



using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IUserDataService
{

    UserViewModel GetUser();


    void Initialize();


    event EventHandler<UserViewModel> UserDataUpdated;
}