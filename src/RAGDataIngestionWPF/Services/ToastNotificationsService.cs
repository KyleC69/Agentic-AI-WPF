// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         ToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 212937



using Windows.UI.Notifications;

using Microsoft.Toolkit.Uwp.Notifications;

using AgenticAIWPF.Contracts.Services;




namespace AgenticAIWPF.Services;





public sealed partial class ToastNotificationsService : IToastNotificationsService
{

    public void ShowToastNotification(ToastNotification toastNotification)
    {
        ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
    }
}