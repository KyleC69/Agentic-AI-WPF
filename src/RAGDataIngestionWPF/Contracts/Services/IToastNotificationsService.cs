// Build Date: 2026/03/29
// Solution: File
// Project:   RAGDataIngestionWPF
// File:         IToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 051949



using Windows.UI.Notifications;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IToastNotificationsService
{
    void ShowToastNotification(ToastNotification toastNotification);


    void ShowToastNotificationSample();
}