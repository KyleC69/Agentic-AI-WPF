// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 194528



using Windows.UI.Notifications;




namespace AgenticAIWPF.Contracts.Services;





public interface IToastNotificationsService
{
    void ShowToastNotification(ToastNotification toastNotification);

    void ShowToastNotification(string title, string message);


    void ShowToastNotificationSample();
}