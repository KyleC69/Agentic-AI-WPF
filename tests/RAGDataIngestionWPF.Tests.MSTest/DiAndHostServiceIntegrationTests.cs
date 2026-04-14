// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF.Tests.MSTest
// File:         DiAndHostServiceIntegrationTests.cs
// Author: Kyle L. Crowder
// Build Num: 212956



using System.Windows;
using System.Windows.Controls;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Moq;

using RAGDataIngestionWPF.Activation;
using RAGDataIngestionWPF.Contracts.Activation;
using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Contracts.Views;
using RAGDataIngestionWPF.Services;




namespace RAGDataIngestionWPF.Tests.MSTest;





[TestClass]
public class DiAndHostServiceIntegrationTests
{

    [TestMethod]
    public void ApplicationHostServiceStartSurfacesMissingThemeResourceFailure()
    {
        StaTestHelper.Run(() =>
        {
            Mock<INavigationService> navigation = new();
            Mock<IToastNotificationsService> toast = new();
            Mock<IUserDataService> userData = new();
            Mock<IRuntimeAppSettingsService> runtimeSettings = new();
            Mock<IShellWindow> shellWindow = new();
            shellWindow.Setup(window => window.GetNavigationFrame()).Returns(new Frame());

            ServiceProvider provider = new ServiceCollection().AddSingleton(shellWindow.Object).BuildServiceProvider();

            ApplicationHostService service = new(provider, Array.Empty<IActivationHandler>(), navigation.Object, toast.Object, userData.Object, runtimeSettings.Object);

            Exception captured = null;
            try
            {
                service.StartAsync(CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                captured = ex;
            }

            Assert.IsNotNull(captured);

            userData.Verify(s => s.Initialize(), Times.Never);
            shellWindow.Verify(s => s.ShowWindow(), Times.Never);
            navigation.Verify(s => s.Initialize(It.IsAny<Frame>()), Times.Never);
            toast.Verify(s => s.ShowToastNotificationSample(), Times.Never);
        });
    }








    [TestMethod]
    public void AppRegistrationMethodsRegisterExpectedServices()
    {
        ServiceCollection services = [];

        _ = services.AddHostServicesModule();
        _ = services.AddActivationHandlersModule();
        _ = services.AddCoreServicesModule();
        _ = services.AddApplicationServicesModule();
        _ = services.AddViewsAndViewModelsModule();

        AssertHasSingleton<IHostedService>(services);
        AssertHasSingleton<IActivationHandler>(services);
        AssertHasSingleton<IToastNotificationsService>(services);
        AssertHasSingleton<IPageService>(services);
        AssertHasSingleton<INavigationService>(services);
        AssertHasSingleton<IUserDataService>(services);

        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IShellWindow) && d.Lifetime == ServiceLifetime.Transient));
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(ViewModels.MainViewModel) && d.Lifetime == ServiceLifetime.Transient));
    }








    private static void AssertHasSingleton<TService>(IServiceCollection services)
    {
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(TService) && d.Lifetime == ServiceLifetime.Singleton));
    }
















    [TestMethod]
    public void ToastNotificationActivationCanHandleBasedOnConfiguration()
    {
        IConfiguration canHandleConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { [ToastNotificationActivationHandler.ACTIVATION_ARGUMENTS] = "args" }).Build();

        IConfiguration cannotHandleConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>()).Build();

        ToastNotificationActivationHandler positive = new(canHandleConfig);
        ToastNotificationActivationHandler negative = new(cannotHandleConfig);

        Assert.IsTrue(positive.CanHandle());
        Assert.IsFalse(negative.CanHandle());
    }








    [TestMethod]
    public void ToastNotificationActivationRestoresMinimizedMainWindow()
    {
        StaTestHelper.Run(() =>
        {
            Application app = new() { ShutdownMode = ShutdownMode.OnExplicitShutdown };

            try
            {
                IConfiguration config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { [ToastNotificationActivationHandler.ACTIVATION_ARGUMENTS] = "args" }).Build();
                ToastNotificationActivationHandler handler = new(config);

                TestShellWindow shell = new();
                shell.Show();
                app.MainWindow = shell;
                shell.WindowState = WindowState.Minimized;

                handler.HandleAsync().GetAwaiter().GetResult();

                Assert.AreEqual(WindowState.Normal, shell.WindowState);
                shell.Close();
            }
            finally
            {
                app.Shutdown();
            }
        });
    }








    private sealed class TestShellWindow : Window, IShellWindow
    {
        private readonly Frame _frame = new();








        public void CloseWindow()
        {
            this.Close();
        }








        public Frame GetNavigationFrame()
        {
            return _frame;
        }








        public void ShowWindow()
        {
            this.Show();
        }
    }
}