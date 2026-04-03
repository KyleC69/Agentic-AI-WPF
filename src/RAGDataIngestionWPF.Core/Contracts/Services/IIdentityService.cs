// Build Date: 2026/03/31
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Core
// File:         IIdentityService.cs
// Author: Kyle L. Crowder
// Build Num: 232116



using RAGDataIngestionWPF.Core.Helpers;




namespace RAGDataIngestionWPF.Core.Contracts.Services;





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