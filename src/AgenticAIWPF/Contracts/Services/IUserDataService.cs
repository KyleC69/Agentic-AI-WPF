// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IUserDataService.cs
// Author: Kyle L. Crowder
// Build Num: 194528



using AgenticAIWPF.ViewModels;




namespace AgenticAIWPF.Contracts.Services;





public interface IUserDataService
{

    UserViewModel GetUser();


    void Initialize();


    event EventHandler<UserViewModel> UserDataUpdated;
}