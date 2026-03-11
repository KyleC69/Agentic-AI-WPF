// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         IIdentityService.cs
// Author: Kyle L. Crowder
// Build Num: 105555



using RAGDataIngestionWPF.Core.Helpers;




namespace RAGDataIngestionWPF.Core.Contracts.Services;





public interface IIdentityService
{

    Task<bool> AcquireTokenSilentAsync();


    Task<string> GetAccessTokenAsync(string[] scopes);


    Task<string> GetAccessTokenForGraphAsync();


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