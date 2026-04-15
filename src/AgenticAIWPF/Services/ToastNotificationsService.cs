// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         ToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 194536



using Windows.UI.Notifications;

using AgenticAIWPF.Contracts.Services;

using Microsoft.Toolkit.Uwp.Notifications;




namespace AgenticAIWPF.Services;





public sealed partial class ToastNotificationsService : IToastNotificationsService
{

    public void ShowToastNotification(ToastNotification toastNotification)
    {
        ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
    }
}