// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF.Core
// File:         IdentityService.cs
// Author: Kyle L. Crowder
// Build Num: 051947



using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Helpers;




namespace RAGDataIngestionWPF.Core.Services;





public sealed class IdentityService : IIdentityService
{
    private string _accountUserName = string.Empty;
    private bool _isLoggedIn;

    public event EventHandler LoggedIn;

    public event EventHandler LoggedOut;








    public void InitializeWithAadAndPersonalMsAccounts(string clientId, string redirectUri = null)
    {
        // No-op: identity client removed
    }








    public void InitializeWithPersonalMsAccounts(string clientId, string redirectUri = null)
    {
        // No-op: identity client removed
    }








    public void InitializeWithAadMultipleOrgs(string clientId, bool integratedAuth = false, string redirectUri = null)
    {
        // No-op: identity client removed
    }








    public void InitializeWithAadSingleOrg(string clientId, string tenant, bool integratedAuth = false, string redirectUri = null)
    {
        // No-op: identity client removed
    }








    public bool IsLoggedIn()
    {
        return _isLoggedIn;
    }








    public async Task<LoginResultType> LoginAsync()
    {
        _isLoggedIn = true;
        _accountUserName = Environment.UserName;
        LoggedIn?.Invoke(this, EventArgs.Empty);
        return await Task.FromResult(LoginResultType.Success);
    }








    public bool IsAuthorized()
    {
        return true;
    }








    public string GetAccountUserName()
    {
        return _accountUserName;
    }








    public async Task LogoutAsync()
    {
        if (_isLoggedIn)
        {
            _isLoggedIn = false;
            LoggedOut?.Invoke(this, EventArgs.Empty);
        }

        await Task.CompletedTask;
    }








    public async Task<string> GetAccessTokenAsync(string[] scopes)
    {
        await Task.CompletedTask;
        return string.Empty;
    }








    public async Task<bool> AcquireTokenSilentAsync()
    {
        return await AcquireTokenSilentAsync(Array.Empty<string>());
    }








    private async Task<bool> AcquireTokenSilentAsync(string[] scopes)
    {
        _ = scopes;
        await Task.CompletedTask;
        return false;
    }
}