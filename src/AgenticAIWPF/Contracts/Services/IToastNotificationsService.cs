// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Date: 2026/05/24



using Windows.UI.Notifications;




namespace AgenticAIWPF.Contracts.Services;





public interface IToastNotificationsService
{
    void ShowToastNotification(ToastNotification toastNotification);


    void ShowToastNotification(string title, string message);


    void ShowToastNotificationSample();
}