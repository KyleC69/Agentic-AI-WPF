// Build Date: 2026/03/15
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 091008



using Windows.UI.Notifications;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IToastNotificationsService
{
    void ShowToastNotification(ToastNotification toastNotification);


    void ShowToastNotificationSample();
}