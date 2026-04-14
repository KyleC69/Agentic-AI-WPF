// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IUserDataService.cs
// Author: Kyle L. Crowder
// Build Num: 212930



using AgenticAIWPF.ViewModels;




namespace AgenticAIWPF.Contracts.Services;





public interface IUserDataService
{

    UserViewModel GetUser();


    void Initialize();


    event EventHandler<UserViewModel> UserDataUpdated;
}