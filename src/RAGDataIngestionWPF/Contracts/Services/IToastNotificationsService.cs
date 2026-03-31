// Build Date: 2026/03/30
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 233153



using Windows.UI.Notifications;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IToastNotificationsService
{
    void ShowToastNotification(ToastNotification toastNotification);


    void ShowToastNotificationSample();
}