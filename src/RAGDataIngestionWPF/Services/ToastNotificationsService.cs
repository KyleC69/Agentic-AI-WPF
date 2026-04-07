// Build Date: 2026/04/06
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 212937



using Windows.UI.Notifications;

using Microsoft.Toolkit.Uwp.Notifications;

using RAGDataIngestionWPF.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





public sealed partial class ToastNotificationsService : IToastNotificationsService
{

    public void ShowToastNotification(ToastNotification toastNotification)
    {
        ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
    }
}