// Build Date: 2026/04/03
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 095211



using Windows.UI.Notifications;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IToastNotificationsService
{
    void ShowToastNotification(ToastNotification toastNotification);


    void ShowToastNotificationSample();
}