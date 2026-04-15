// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         UserDataService.cs
// Author: Kyle L. Crowder
// Build Num: 194537



using AgenticAIWPF.Contracts.Services;
using AgenticAIWPF.ViewModels;




namespace AgenticAIWPF.Services;





/// <summary>
///     Provides minimal runtime user information used by the settings page.
/// </summary>
public sealed class UserDataService : IUserDataService
{
    private UserViewModel _currentUser = new();








    public UserViewModel GetUser()
    {
        return _currentUser;
    }








    public void Initialize()
    {
        _currentUser = new UserViewModel { Name = Environment.UserName, UserPrincipalName = Environment.UserName };

        UserDataUpdated?.Invoke(this, _currentUser);
    }








    public event EventHandler<UserViewModel> UserDataUpdated;
}