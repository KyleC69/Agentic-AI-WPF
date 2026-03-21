using System.IO;
using System.Reflection;

using Microsoft.Extensions.Logging;

using Moq;

using DataIngestionLib.Contracts;
using DataIngestionLib.Services;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.Settings;
using RAGDataIngestionWPF.Services;
using RAGDataIngestionWPF.ViewModels;

namespace RAGDataIngestionWPF.Tests.MSTest;

[TestClass]
public class ConfigurationAndEdgeCaseServiceTests
{
    [TestMethod]
    public void AppSettingsGetTokenBudgetAggregatesConfiguredBudgets()
    {
        AppSettings settings = new();

        int oldSession = settings.SessionBudget;
        int oldSystem = settings.SystemBudget;
        int oldRag = settings.RAGBudget;
        int oldTool = settings.ToolBudget;
        int oldMeta = settings.MetaBudget;
        int oldContext = settings.MaximumContext;

        try
        {
            settings.SessionBudget = 10;
            settings.SystemBudget = 20;
            settings.RAGBudget = 30;
            settings.ToolBudget = 40;
            settings.MetaBudget = 50;
            settings.MaximumContext = 900;

            DataIngestionLib.Services.Contracts.TokenBudget budget = settings.GetTokenBudget();

            Assert.AreEqual(10, budget.SessionBudget);
            Assert.AreEqual(20, budget.SystemBudget);
            Assert.AreEqual(30, budget.RAGBudget);
            Assert.AreEqual(40, budget.ToolBudget);
            Assert.AreEqual(50, budget.MetaBudget);
            Assert.AreEqual(900, budget.MaximumContext);
            Assert.AreEqual(150, budget.BudgetTotal);
        }
        finally
        {
            settings.SessionBudget = oldSession;
            settings.SystemBudget = oldSystem;
            settings.RAGBudget = oldRag;
            settings.ToolBudget = oldTool;
            settings.MetaBudget = oldMeta;
            settings.MaximumContext = oldContext;
        }
    }

    [TestMethod]
    public void AppSettingsStringSettersNormalizeNullToEmpty()
    {
        AppSettings settings = new();
        string oldChatModel = settings.ChatModel;
        string oldAgentId = settings.AgentId;

        try
        {
            settings.ChatModel = null;
            settings.AgentId = null;
            Assert.AreEqual(string.Empty, settings.ChatModel);
            Assert.AreEqual(string.Empty, settings.AgentId);
        }
        finally
        {
            settings.ChatModel = oldChatModel;
            settings.AgentId = oldAgentId;
        }
    }

    [TestMethod]
    public void AgentIdentityProviderReturnsConfiguredAgentId()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(s => s.AgentId).Returns("  configured-agent  ");

        AgentIdentityProvider provider = new(settings.Object);

        Assert.AreEqual("configured-agent", provider.GetAgentId());
    }

    [TestMethod]
    public void AgentIdentityProviderFallsBackWhenConfiguredAgentIdIsBlank()
    {
        Mock<IAppSettings> settings = new();
        settings.SetupGet(s => s.AgentId).Returns(string.Empty);

        AgentIdentityProvider provider = new(settings.Object);

        Assert.AreEqual("Agentic-Max", provider.GetAgentId());
    }

    [TestMethod]
    public void IdentityCacheServiceRoundTripsEncryptedToken()
    {
        Type serviceType = Type.GetType("RAGDataIngestionWPF.Services.IdentityCacheService, RAGDataIngestionWPF");
        Assert.IsNotNull(serviceType);

        object service = Activator.CreateInstance(serviceType, true);

        FieldInfo pathField = serviceType.GetField("MsalCacheFilePath", BindingFlags.Public | BindingFlags.Static);
        FieldInfo fileField = serviceType.GetField("MsalCacheFileName", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(pathField);
        Assert.IsNotNull(fileField);

        string folder = (string)pathField.GetValue(null);
        string file = (string)fileField.GetValue(null);
        string fullPath = Path.Combine(folder, file);

        byte[] previous = File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : null;

        try
        {
            byte[] token = [1, 2, 3, 4, 5, 6];
            _ = serviceType.GetMethod("SaveMsalToken", BindingFlags.Public | BindingFlags.Instance).Invoke(service, new object[] { token });
            byte[] roundTrip = (byte[])serviceType.GetMethod("ReadMsalToken", BindingFlags.Public | BindingFlags.Instance).Invoke(service, null);

            CollectionAssert.AreEqual(token, roundTrip);
        }
        finally
        {
            if (previous == null)
            {
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            else
            {
                _ = Directory.CreateDirectory(folder);
                File.WriteAllBytes(fullPath, previous);
            }
        }
    }

    [TestMethod]
    public void SettingsViewModelPrivateParseMethodsHandleEdgeCases()
    {
        Type vmType = typeof(SettingsViewModel);

        MethodInfo parseBool = vmType.GetMethod("ParseBool", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo parseInt = vmType.GetMethod("ParseInt", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo parseNullableInt = vmType.GetMethod("ParseNullableInt", BindingFlags.NonPublic | BindingFlags.Static);
        MethodInfo parseTheme = vmType.GetMethod("ParseTheme", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.IsTrue((bool)parseBool.Invoke(null, new object[] { "true", false }));
        Assert.IsFalse((bool)parseBool.Invoke(null, new object[] { "invalid", false }));

        Assert.AreEqual(12, (int)parseInt.Invoke(null, new object[] { "12", 3, 1 }));
        Assert.AreEqual(3, (int)parseInt.Invoke(null, new object[] { "0", 3, 1 }));

        Assert.IsNull(parseNullableInt.Invoke(null, new object[] { "", 5 }));
        Assert.AreEqual(5, (int)parseNullableInt.Invoke(null, new object[] { "-4", 5 }));

        object theme = parseTheme.Invoke(null, new object[] { "NotATheme" });
        Assert.AreEqual(RAGDataIngestionWPF.Models.AppTheme.Default, (RAGDataIngestionWPF.Models.AppTheme)theme);
    }

    [TestMethod]
    public void SettingsViewModelSetThemeCommandCanSurfaceMissingThemeResourceEdgeCase()
    {
        StaTestHelper.Run(() =>
        {
            Mock<ISystemService> system = new();
            Mock<IApplicationInfoService> appInfo = new();
            appInfo.Setup(service => service.GetVersion()).Returns(new Version(1, 2, 3, 4));
            Mock<IUserDataService> userData = new();

            SettingsViewModel viewModel = new(system.Object, appInfo.Object, userData.Object);

            Exception captured = null;
            try
            {
                viewModel.SetThemeCommand.Execute("Dark");
            }
            catch (Exception ex)
            {
                captured = ex;
            }

            Assert.IsNotNull(captured);
        });
    }

    [TestMethod]
    public void SystemServiceOpenInWebBrowserWithNullUrlThrows()
    {
        SystemService service = new();

        Exception captured = null;
        try
        {
            service.OpenInWebBrowser(null);
        }
        catch (Exception ex)
        {
            captured = ex;
        }

        Assert.IsNotNull(captured);
    }
}
