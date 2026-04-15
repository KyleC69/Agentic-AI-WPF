// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Core
// File:         IIdentityService.cs
// Author: Kyle L. Crowder
// Build Num: 194525



using AgenticAIWPF.Core.Helpers;




namespace AgenticAIWPF.Core.Contracts.Services;





public interface IIdentityService
{

    Task<bool> AcquireTokenSilentAsync();


    Task<string> GetAccessTokenAsync(string[] scopes);


    string GetAccountUserName();


    void InitializeWithAadAndPersonalMsAccounts(string clientId, string redirectUri = null);


    void InitializeWithAadMultipleOrgs(string clientId, bool integratedAuth = false, string redirectUri = null);


    void InitializeWithAadSingleOrg(string clientId, string tenant, bool integratedAuth = false, string redirectUri = null);


    void InitializeWithPersonalMsAccounts(string clientId, string redirectUri = null);


    bool IsAuthorized();


    bool IsLoggedIn();


    event EventHandler LoggedIn;

    event EventHandler LoggedOut;


    Task<LoginResultType> LoginAsync();


    Task LogoutAsync();
}