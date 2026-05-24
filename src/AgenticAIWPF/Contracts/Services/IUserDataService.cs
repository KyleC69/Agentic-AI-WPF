// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IUserDataService.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using AgenticAIWPF.ViewModels;




namespace AgenticAIWPF.Contracts.Services;





public interface IUserDataService
{

    UserViewModel GetUser();


    void Initialize();


    event EventHandler<UserViewModel> UserDataUpdated;
}