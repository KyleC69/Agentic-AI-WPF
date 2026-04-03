// Build Date: 2026/03/31
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         UserDataService.cs
// Author: Kyle L. Crowder
// Build Num: 232126



using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Services;





/// <summary>
///     Provides minimal runtime user information used by the settings page.
/// </summary>
public sealed class UserDataService : IUserDataService
{
    private UserViewModel _currentUser = new();

    public event EventHandler<UserViewModel> UserDataUpdated;








    public UserViewModel GetUser()
    {
        return _currentUser;
    }








    public void Initialize()
    {
        _currentUser = new UserViewModel { Name = Environment.UserName, UserPrincipalName = Environment.UserName };

        UserDataUpdated?.Invoke(this, _currentUser);
    }
}