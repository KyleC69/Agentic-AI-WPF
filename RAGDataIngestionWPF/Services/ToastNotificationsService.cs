// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 175111



using Microsoft.Toolkit.Uwp.Notifications;

using RAGDataIngestionWPF.Contracts.Services;

using Windows.UI.Notifications;




namespace RAGDataIngestionWPF.Services;





public partial class ToastNotificationsService : IToastNotificationsService
{

    public void ShowToastNotification(ToastNotification toastNotification)
    {
        ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
    }
}