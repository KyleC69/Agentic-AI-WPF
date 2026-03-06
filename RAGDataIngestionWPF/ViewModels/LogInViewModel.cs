// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         LogInViewModel.cs
//   Author: Kyle L. Crowder



using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Helpers;
using RAGDataIngestionWPF.Properties;




namespace RAGDataIngestionWPF.ViewModels;





public class LogInViewModel : ObservableObject
{
    private readonly IIdentityService _identityService;

    public LogInViewModel(IIdentityService identityService)
    {
        _identityService = identityService;
    }








    public bool IsBusy
    {
        get;
        set
        {
            SetProperty(ref field, value);
            LoginCommand.NotifyCanExecuteChanged();
        }
    }





    public RelayCommand LoginCommand => field ??= new RelayCommand(OnLogin, () => !IsBusy);





    public string StatusMessage
    {
        get; set => SetProperty(ref field, value);
    }








    private string GetStatusMessage(LoginResultType loginResult)
    {
        return loginResult switch
        {
            LoginResultType.Unauthorized => Resources.StatusUnauthorized,
            LoginResultType.NoNetworkAvailable => Resources.StatusNoNetworkAvailable,
            LoginResultType.UnknownError => Resources.StatusLoginFails,
            LoginResultType.Success or LoginResultType.CancelledByUser => string.Empty,
            _ => string.Empty,
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