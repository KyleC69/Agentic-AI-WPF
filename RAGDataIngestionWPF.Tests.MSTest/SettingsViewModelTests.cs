using DataIngestionLib.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Core.Models;
using RAGDataIngestionWPF.Models;
using RAGDataIngestionWPF.ViewModels;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class SettingsViewModelTests
{
    [TestMethod]
    public void OnNavigatedTo_LoadsChatModelNameFromChatHistorySettings()
    {
        SettingsViewModel viewModel = CreateViewModel(
                chatSettings: new ChatHistoryOptions
                {
                    ChatModelName = "model-x",
                    ConnectionString = "Server=.;Database=AIChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;",
                    EmbeddingsModelName = "embed-x",
                    MaxContextMessages = 50,
                    MaxContextTokens = 150000,
                    RAGKnowledgeEnabled = true,
                    ChatHistoryContextEnabled = true
                });

        viewModel.OnNavigatedTo(null!);

        Assert.AreEqual("model-x", viewModel.ChatModelName);
    }

    [TestMethod]
    public void SaveChatHistorySettingsCommand_PersistsChatModelName()
    {
        Mock<IChatHistorySettingsService> chatSettingsServiceMock = CreateChatHistorySettingsServiceMock();
        SettingsViewModel viewModel = CreateViewModel(chatSettingsServiceMock: chatSettingsServiceMock);

        viewModel.OnNavigatedTo(null!);
        viewModel.ChatModelName = "saved-model";

        viewModel.SaveChatHistorySettingsCommand.Execute(null);

        chatSettingsServiceMock.Verify(
                service => service.SaveSettings(It.Is<ChatHistoryOptions>(options => options.ChatModelName == "saved-model")),
                Times.Once);
    }

    [TestMethod]
    public void SaveChatHistorySettingsCommand_SetsStatusMessage()
    {
        SettingsViewModel viewModel = CreateViewModel();

        viewModel.OnNavigatedTo(null!);
        viewModel.SaveChatHistorySettingsCommand.Execute(null);

        Assert.AreEqual(viewModel.ChatHistorySaveStatusText, viewModel.ChatHistorySettingsStatus);
    }

    [TestMethod]
    public void OnNavigatedTo_LoadsMinimumLogLevelFromLoggingLevelService()
    {
        Mock<ILoggingLevelService> loggingLevelServiceMock = new();
        loggingLevelServiceMock.Setup(s => s.GetMinimumLevel()).Returns(LogLevel.Warning);

        SettingsViewModel viewModel = CreateViewModel(loggingLevelServiceMock: loggingLevelServiceMock);
        viewModel.OnNavigatedTo(null!);

        Assert.AreEqual(LogLevel.Warning, viewModel.MinimumLogLevel);
    }

    [TestMethod]
    public void SetLogLevelCommand_DelegatesToLoggingLevelService()
    {
        Mock<ILoggingLevelService> loggingLevelServiceMock = new();
        loggingLevelServiceMock.Setup(s => s.GetMinimumLevel()).Returns(LogLevel.Trace);

        SettingsViewModel viewModel = CreateViewModel(loggingLevelServiceMock: loggingLevelServiceMock);
        viewModel.OnNavigatedTo(null!);

        viewModel.MinimumLogLevel = LogLevel.Error;
        viewModel.SetLogLevelCommand.Execute(null);

        loggingLevelServiceMock.Verify(s => s.SetMinimumLevel(LogLevel.Error), Times.Once);
    }

    [TestMethod]
    public void AvailableLogLevels_DoesNotContainNone()
    {
        SettingsViewModel viewModel = CreateViewModel();

        Assert.IsFalse(viewModel.AvailableLogLevels.Contains(LogLevel.None));
    }

    private static Mock<IChatHistorySettingsService> CreateChatHistorySettingsServiceMock()
    {
        Mock<IChatHistorySettingsService> chatSettingsServiceMock = new();
        chatSettingsServiceMock
                .Setup(service => service.GetCurrentSettings())
                .Returns(new ChatHistoryOptions
                {
                    ChatModelName = "gpt-oss:20b-cloud",
                    ConnectionString = "Server=.;Database=AIChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;",
                    EmbeddingsModelName = "mxbai-embed-large-v1:latest",
                    MaxContextMessages = 40,
                    MaxContextTokens = 120000,
                    RAGKnowledgeEnabled = true,
                    ChatHistoryContextEnabled = true
                });

        return chatSettingsServiceMock;
    }

    private static SettingsViewModel CreateViewModel(
            ChatHistoryOptions? chatSettings = null,
            Mock<IChatHistorySettingsService>? chatSettingsServiceMock = null,
            Mock<ILoggingLevelService>? loggingLevelServiceMock = null)
    {
        Mock<IThemeSelectorService> themeSelectorServiceMock = new();
        themeSelectorServiceMock.Setup(service => service.GetCurrentTheme()).Returns(AppTheme.Dark);

        Mock<IOptions<AppSettings>> appConfigMock = new();
        appConfigMock.SetupGet(options => options.Value).Returns(new AppSettings
        {
            PrivacyStatement = "https://example.test/privacy",
            ConfigurationsFolder = "RAGDataIngestionWPF\\Configurations",
            ChatSessionFileName = "ChatSession.json",
            AppPropertiesFileName = "AppProperties.json",
            UserFileName = "User.json"
        });

        Mock<ISystemService> systemServiceMock = new();
        Mock<IApplicationInfoService> applicationInfoServiceMock = new();
        applicationInfoServiceMock.Setup(service => service.GetVersion()).Returns(new Version(1, 0, 0, 0));

        Mock<IUserDataService> userDataServiceMock = new();
        userDataServiceMock.Setup(service => service.GetUser()).Returns(new UserViewModel { Name = "User" });

        Mock<IApplicationIdService> applicationIdServiceMock = new();
        applicationIdServiceMock.Setup(service => service.GetApplicationId()).Returns(Guid.NewGuid());

        Mock<IChatHistorySettingsService> resolvedChatSettingsServiceMock = chatSettingsServiceMock ?? new Mock<IChatHistorySettingsService>();
        resolvedChatSettingsServiceMock
                .Setup(service => service.GetCurrentSettings())
                .Returns(chatSettings ?? new ChatHistoryOptions
                {
                    ChatModelName = "gpt-oss:20b-cloud",
                    ConnectionString = "Server=.;Database=AIChatHistory;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;",
                    EmbeddingsModelName = "mxbai-embed-large-v1:latest",
                    MaxContextMessages = 40,
                    MaxContextTokens = 120000,
                    RAGKnowledgeEnabled = true,
                    ChatHistoryContextEnabled = true
                });

        Mock<ILoggingLevelService> resolvedLoggingLevelServiceMock = loggingLevelServiceMock ?? new Mock<ILoggingLevelService>();
        resolvedLoggingLevelServiceMock
                .Setup(s => s.GetMinimumLevel())
                .Returns(LogLevel.Trace);

        return new SettingsViewModel(
                appConfigMock.Object,
                themeSelectorServiceMock.Object,
                systemServiceMock.Object,
                applicationInfoServiceMock.Object,
                userDataServiceMock.Object,
                applicationIdServiceMock.Object,
                resolvedChatSettingsServiceMock.Object,
                resolvedLoggingLevelServiceMock.Object);
    }
}
