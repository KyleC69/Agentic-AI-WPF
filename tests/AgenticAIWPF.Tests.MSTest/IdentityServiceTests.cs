// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF.Tests.MSTest
// File:         IdentityServiceTests.cs
// Author: Kyle L. Crowder
// Build Num: 212958



using AgenticAIWPF.Core.Helpers;
using AgenticAIWPF.Core.Services;




namespace AgenticAIWPF.Tests.MSTest;





[TestClass]
public class IdentityServiceTests
{

    [TestMethod]
    public async Task AcquireTokenSilentAsyncReturnsFalse()
    {
        IdentityService service = new();

        var acquired = await service.AcquireTokenSilentAsync();

        Assert.IsFalse(acquired);
    }








    [TestMethod]
    public async Task GetAccessTokenAsyncReturnsEmptyString()
    {
        IdentityService service = new IdentityService();

        var token = await service.GetAccessTokenAsync(["scope"]);

        Assert.AreEqual(string.Empty, token);
    }








    [TestMethod]
    public void IsAuthorizedAlwaysReturnsTrue()
    {
        IdentityService service = new IdentityService();

        var authorized = service.IsAuthorized();

        Assert.IsTrue(authorized);
    }








    [TestMethod]
    public async Task LoginAsyncSetsStateAndRaisesLoggedInEvent()
    {
        IdentityService service = new IdentityService();
        var eventRaised = false;
        service.LoggedIn += (_, _) => eventRaised = true;

        LoginResultType result = await service.LoginAsync();

        Assert.AreEqual(LoginResultType.Success, result);
        Assert.IsTrue(service.IsLoggedIn());
        Assert.IsFalse(string.IsNullOrWhiteSpace(service.GetAccountUserName()));
        Assert.IsTrue(eventRaised);
    }








    [TestMethod]
    public async Task LogoutAsyncAfterLoginClearsStateAndRaisesLoggedOutEvent()
    {
        IdentityService service = new IdentityService();
        var eventRaised = false;
        service.LoggedOut += (_, _) => eventRaised = true;
        _ = await service.LoginAsync();

        await service.LogoutAsync();

        Assert.IsFalse(service.IsLoggedIn());
        Assert.IsTrue(eventRaised);
    }








    [TestMethod]
    public async Task LogoutAsyncWhenNotLoggedInDoesNotRaiseEvent()
    {
        IdentityService service = new IdentityService();
        var eventRaised = false;
        service.LoggedOut += (_, _) => eventRaised = true;

        await service.LogoutAsync();

        Assert.IsFalse(eventRaised);
    }
}