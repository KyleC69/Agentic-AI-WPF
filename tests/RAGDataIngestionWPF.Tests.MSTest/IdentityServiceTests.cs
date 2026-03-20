using RAGDataIngestionWPF.Core.Helpers;
using RAGDataIngestionWPF.Core.Services;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class IdentityServiceTests
{
    [TestMethod]
    public async Task LoginAsyncSetsStateAndRaisesLoggedInEvent()
    {
        IdentityService service = new();
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
        IdentityService service = new();
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
        IdentityService service = new();
        var eventRaised = false;
        service.LoggedOut += (_, _) => eventRaised = true;

        await service.LogoutAsync();

        Assert.IsFalse(eventRaised);
    }

    [TestMethod]
    public void IsAuthorizedAlwaysReturnsTrue()
    {
        IdentityService service = new();

        bool authorized = service.IsAuthorized();

        Assert.IsTrue(authorized);
    }

    [TestMethod]
    public async Task GetAccessTokenAsyncReturnsEmptyString()
    {
        IdentityService service = new();

        string token = await service.GetAccessTokenAsync(["scope"]);

        Assert.AreEqual(string.Empty, token);
    }

    [TestMethod]
    public async Task AcquireTokenSilentAsyncReturnsFalse()
    {
        IdentityService service = new();

        bool acquired = await service.AcquireTokenSilentAsync();

        Assert.IsFalse(acquired);
    }
}
