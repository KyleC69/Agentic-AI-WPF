// Build Date: 2026/04/06
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         IToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 212930



using Windows.UI.Notifications;




namespace AgenticAIWPF.Contracts.Services;





public interface IToastNotificationsService
{
    void ShowToastNotification(ToastNotification toastNotification);


    void ShowToastNotificationSample();
}