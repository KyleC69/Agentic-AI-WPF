// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         LogInViewModel.cs
// Author: Kyle L. Crowder
// Build Num: 091017



using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Helpers;
using RAGDataIngestionWPF.Properties;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class LogInViewModel(IIdentityService identityService) : ObservableObject
{
    private readonly IIdentityService _identityService = identityService;





    public bool IsBusy
    {
        get;
        set
        {
            _ = this.SetProperty(ref field, value);
            LoginCommand.NotifyCanExecuteChanged();
        }
    }





  
    public RelayCommand LoginCommand
    {
        get { return field ??= new RelayCommand(OnLogin, () => !IsBusy); }
    }





    [ObservableProperty] private string statusMessage = string.Empty;








    private static string GetStatusMessage(LoginResultType loginResult)
    {
        return loginResult switch
        {
                LoginResultType.Unauthorized => Resources.StatusUnauthorized,
                LoginResultType.NoNetworkAvailable => Resources.StatusNoNetworkAvailable,
                LoginResultType.UnknownError => Resources.StatusLoginFails,
                _ => string.Empty
        };
    }








    private async void OnLogin()
    {
        IsBusy = true;
        StatusMessage = string.Empty;
        LoginResultType loginResult = await _identityService.LoginAsync();
        StatusMessage = GetStatusMessage(loginResult);
        IsBusy = false;
    }
}