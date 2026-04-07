// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         WpfProjectServiceAndModelTests.cs
// Author: Kyle L. Crowder
// Build Num: 213005



using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

using RAGDataIngestionWPF.Models;
using RAGDataIngestionWPF.Services;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class WpfProjectServiceAndModelTests
{

    [TestMethod]
    public void ApplicationInfoServiceReturnsVersion()
    {
        ApplicationInfoService service = new();

        Version version = service.GetVersion();

        Assert.IsNotNull(version);
        Assert.IsTrue(version.Major >= 0);
    }








    [TestMethod]
    public void AppThemeEnumContainsExpectedValues()
    {
        var names = Enum.GetNames(typeof(AppTheme));

        CollectionAssert.Contains(names, nameof(AppTheme.Default));
        CollectionAssert.Contains(names, nameof(AppTheme.Light));
        CollectionAssert.Contains(names, nameof(AppTheme.Dark));
    }








    [TestMethod]
    public void LoggingLevelSwitchCanBeUpdated()
    {
        LoggingLevelSwitch levelSwitch = new();

        levelSwitch.MinimumLevel = LogLevel.Warning;

        Assert.AreEqual(LogLevel.Warning, levelSwitch.MinimumLevel);
    }








    [TestMethod]
    public void LoggingLevelSwitchDefaultsToTrace()
    {
        LoggingLevelSwitch levelSwitch = new();

        Assert.AreEqual(LogLevel.Trace, levelSwitch.MinimumLevel);
    }








    [TestMethod]
    public void MessageDisplayCanStoreTimestamp()
    {
        DateTime now = DateTime.UtcNow;
        MessageDisplay display = new() { Role = ChatRole.User, Timestamp = now, Text = "t", Message = new ChatMessage(ChatRole.User, "t") };

        Assert.AreEqual(now, display.Timestamp);
        Assert.AreEqual("t", display.Text);
    }








    [TestMethod]
    public void MessageDisplayIsUserTracksRole()
    {
        MessageDisplay user = new() { Role = ChatRole.User, Text = "hello", Message = new ChatMessage(ChatRole.User, "hello") };
        MessageDisplay assistant = new() { Role = ChatRole.Assistant, Text = "reply", Message = new ChatMessage(ChatRole.Assistant, "reply") };

        Assert.IsTrue(user.IsUser);
        Assert.IsFalse(assistant.IsUser);
    }








    [TestMethod]
    public void PersistAndRestoreServiceMethodsDoNotThrow()
    {
        PersistAndRestoreService service = new();

        service.PersistData();
        service.RestoreData();

        Assert.IsNotNull(service);
    }








    [TestMethod]
    public void UserDataServiceGetUserReturnsDefaultUserBeforeInitialize()
    {
        UserDataService service = new();

        UserViewModel user = service.GetUser();

        Assert.IsNotNull(user);
        Assert.AreEqual(string.Empty, user.Name);
        Assert.AreEqual(string.Empty, user.UserPrincipalName);
    }








    [TestMethod]
    public void UserDataServiceInitializeUpdatesUserAndRaisesEvent()
    {
        UserDataService service = new UserDataService();
        var eventRaised = false;
        service.UserDataUpdated += (_, user) =>
        {
            eventRaised = true;
            Assert.AreEqual(Environment.UserName, user.Name);
            Assert.AreEqual(Environment.UserName, user.UserPrincipalName);
        };

        service.Initialize();

        UserViewModel current = service.GetUser();
        Assert.IsTrue(eventRaised);
        Assert.AreEqual(Environment.UserName, current.Name);
        Assert.AreEqual(Environment.UserName, current.UserPrincipalName);
    }
}