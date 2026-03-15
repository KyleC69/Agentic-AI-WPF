// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ToastNotificationActivationHandler.cs
// Author: Kyle L. Crowder
// Build Num: 091007



using System.Windows;

using Microsoft.Extensions.Configuration;

using RAGDataIngestionWPF.Contracts.Activation;
using RAGDataIngestionWPF.Contracts.Views;




namespace RAGDataIngestionWPF.Activation;





// For more information about sending a local toast notification from C# apps, see
// https://docs.microsoft.com/windows/apps/design/shell/tiles-and-notifications/send-local-toast?tabs=desktop
// and https://github.com/microsoft/TemplateStudio/blob/main/docs/WPF/features/toast-notifications.md
public sealed class ToastNotificationActivationHandler : IActivationHandler
{

    private readonly IConfiguration _config;
    public const string ACTIVATION_ARGUMENTS = "ToastNotificationActivationArguments";








    public ToastNotificationActivationHandler(IConfiguration config)
    {
        _config = config;
    }








    public bool CanHandle()
    {
        return !string.IsNullOrEmpty(_config[ACTIVATION_ARGUMENTS]);
    }








    public async Task HandleAsync()
    {
        if (!Application.Current.Windows.OfType<IShellWindow>().Any())
        {
            // Here you can get an instance of the ShellWindow and choose navigate
            // to a specific page depending on the toast notification arguments
        }
        else
        {
            if (Application.Current.MainWindow != null)
            {
                _ = Application.Current.MainWindow.Activate();
                if (Application.Current.MainWindow.WindowState == WindowState.Minimized)
                {
                    Application.Current.MainWindow.WindowState = WindowState.Normal;
                }
            }

        }

        await Task.CompletedTask;
    }
}