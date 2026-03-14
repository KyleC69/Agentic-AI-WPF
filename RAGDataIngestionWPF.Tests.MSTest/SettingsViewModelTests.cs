// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         SettingsViewModelTests.cs
// Author: Kyle L. Crowder
// Build Num: 175105



using Microsoft.Extensions.Logging;

using Moq;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Models;
using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class SettingsViewModelTests
{

    [TestMethod]
    public void AvailableLogLevels_DoesNotContainNone()
    {
        SettingsViewModel viewModel = CreateViewModel();

        Assert.IsFalse(viewModel.AvailableLogLevels.Contains(LogLevel.None));
    }








    private static SettingsViewModel CreateViewModel()
    {
        EnsureAppSetting("ChatModelName", "gpt-oss:20b-cloud");
        EnsureAppSetting("ChatHistoryConnectionString", "Server=.;Database=AIChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");
        EnsureAppSetting("EmbeddingsModelName", "mxbai-embed-large-v1:latest");
        EnsureAppSetting("MaxContextMessages", "40");
        EnsureAppSetting("MaxContextTokens", "120000");
        EnsureAppSetting("RagKnowledgeEnabled", bool.TrueString);
        EnsureAppSetting("ChatHistoryContextEnabled", bool.TrueString);
        EnsureAppSetting("MinimumLogLevel", LogLevel.Trace.ToString());
        EnsureAppSetting("Theme", "Dark");
        EnsureAppSetting("PrivacyStatement", "https://example.test/privacy");

        LoggingLevelSwitch loggingLevelSwitch = new();
        Mock<ISystemService> systemServiceMock = new();
        Mock<IApplicationInfoService> applicationInfoServiceMock = new();
        applicationInfoServiceMock.Setup(service => service.GetVersion()).Returns(new Version(1, 0, 0, 0));

        Mock<IUserDataService> userDataServiceMock = new();
        userDataServiceMock.Setup(service => service.GetUser()).Returns(new UserViewModel { Name = "User" });

        return new SettingsViewModel(
                loggingLevelSwitch,
                systemServiceMock.Object,
                applicationInfoServiceMock.Object,
                userDataServiceMock.Object);
    }








    private static void EnsureAppSetting(string key, string value)
    {
        if (System.Configuration.ConfigurationManager.AppSettings[key] is null)
        {
            SetAppSetting(key, value);
        }
    }








    [TestMethod]
    public void OnNavigatedTo_LoadsChatModelNameFromChatHistorySettings()
    {
        SetAppSetting("ChatModelName", "model-x");
        SettingsViewModel viewModel = CreateViewModel();

        viewModel.OnNavigatedTo(null!);

        Assert.AreEqual("model-x", viewModel.ChatModelName);
    }








    [TestMethod]
    public void OnNavigatedTo_LoadsMinimumLogLevelFromLoggingLevelService()
    {
        SetAppSetting("MinimumLogLevel", LogLevel.Warning.ToString());

        SettingsViewModel viewModel = CreateViewModel();
        viewModel.OnNavigatedTo(null!);

        Assert.AreEqual(LogLevel.Warning, viewModel.MinimumLogLevel);
    }








    [TestMethod]
    public void SaveChatHistorySettingsCommand_PersistsChatModelName()
    {
        SettingsViewModel viewModel = CreateViewModel();

        viewModel.OnNavigatedTo(null!);
        viewModel.ChatModelName = "saved-model";

        viewModel.SaveChatHistorySettingsCommand.Execute(null);

        Assert.AreEqual("saved-model", System.Configuration.ConfigurationManager.AppSettings["ChatModelName"]);
    }








    [TestMethod]
    public void SaveChatHistorySettingsCommand_SetsStatusMessage()
    {
        SettingsViewModel viewModel = CreateViewModel();

        viewModel.OnNavigatedTo(null!);
        viewModel.SaveChatHistorySettingsCommand.Execute(null);

        Assert.AreEqual(viewModel.ChatHistorySaveStatusText, viewModel.ChatHistorySettingsStatus);
    }








    private static void SetAppSetting(string key, string value)
    {
        System.Configuration.Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
        if (config.AppSettings.Settings[key] is null)
        {
            config.AppSettings.Settings.Add(key, value);
        }
        else
        {
            config.AppSettings.Settings[key].Value = value;
        }

        config.Save(System.Configuration.ConfigurationSaveMode.Modified);
        System.Configuration.ConfigurationManager.RefreshSection("appSettings");
    }








    [TestMethod]
    public void SetLogLevelCommand_DelegatesToLoggingLevelService()
    {
        SetAppSetting("MinimumLogLevel", LogLevel.Trace.ToString());

        SettingsViewModel viewModel = CreateViewModel();
        viewModel.OnNavigatedTo(null!);

        viewModel.MinimumLogLevel = LogLevel.Error;
        viewModel.SetLogLevelCommand.Execute(null);

        Assert.AreEqual(LogLevel.Error.ToString(), System.Configuration.ConfigurationManager.AppSettings["MinimumLogLevel"]);
    }
}