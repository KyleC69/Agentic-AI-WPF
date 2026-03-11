// Build Date: 2026/03/11
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SettingsViewModelTests.cs
// Author: Kyle L. Crowder
// Build Num: 105600



using Microsoft.Extensions.Options;

using Moq;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Models;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class SettingsViewModelTests
{

    [TestMethod]
    public void TestSettingsViewModel_SetCurrentTheme()
    {
        Mock<IThemeSelectorService> mockThemeSelectorService = new();
        mockThemeSelectorService.Setup(mock => mock.GetCurrentTheme()).Returns(AppTheme.Light);
        Mock<IOptions<AppSettings>> mockAppConfig = new();
        Mock<ISystemService> mockSystemService = new();
        Mock<IApplicationInfoService> mockApplicationInfoService = new();
        Mock<IUserDataService> mockUserDataService = new();
        Mock<IIdentityService> mockIdentityService = new();

        //   var settingsVm = new SettingsViewModel(mockAppConfig.Object, mockThemeSelectorService.Object, mockSystemService.Object, mockApplicationInfoService.Object, mockUserDataService.Object, mockIdentityService.Object);
        //     settingsVm.OnNavigatedTo(null);

        //     Assert.AreEqual(AppTheme.Light, settingsVm.Theme);
    }








    [TestMethod]
    public void TestSettingsViewModel_SetCurrentVersion()
    {
        Mock<IThemeSelectorService> mockThemeSelectorService = new();
        Mock<IOptions<AppSettings>> mockAppConfig = new();
        Mock<ISystemService> mockSystemService = new();
        Mock<IApplicationInfoService> mockApplicationInfoService = new();
        Mock<IUserDataService> mockUserDataService = new();
        Mock<IIdentityService> mockIdentityService = new();
        Version testVersion = new(1, 2, 3, 4);
        mockApplicationInfoService.Setup(mock => mock.GetVersion()).Returns(testVersion);

        //     var settingsVm = new SettingsViewModel(mockAppConfig.Object, mockThemeSelectorService.Object, mockSystemService.Object, mockApplicationInfoService.Object, mockUserDataService.Object, mockIdentityService.Object);
        //     settingsVm.OnNavigatedTo(null);

        //      Assert.AreEqual($"RAGDataIngestionWPF - {testVersion}", settingsVm.VersionDescription);
    }








    [TestMethod]
    public void TestSettingsViewModel_SetThemeCommand()
    {
        Mock<IThemeSelectorService> mockThemeSelectorService = new();
        Mock<IOptions<AppSettings>> mockAppConfig = new();
        Mock<ISystemService> mockSystemService = new();
        Mock<IApplicationInfoService> mockApplicationInfoService = new();
        Mock<IUserDataService> mockUserDataService = new();
        Mock<IIdentityService> mockIdentityService = new();

        //      var settingsVm = new SettingsViewModel(mockAppConfig.Object, mockThemeSelectorService.Object, mockSystemService.Object, mockApplicationInfoService.Object, mockUserDataService.Object, mockIdentityService.Object);
        //    settingsVm.SetThemeCommand.Execute(AppTheme.Light.ToString());

        //     mockThemeSelectorService.Verify(mock => mock.SetTheme(AppTheme.Light));
    }
}