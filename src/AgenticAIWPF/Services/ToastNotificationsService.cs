// Build Date: 2026/04/14
// Solution: AgenticAIWPF
// Project:   AgenticAIWPF
// File:         ToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 194536



using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

using AgenticAIWPF.Contracts.Services;

using Microsoft.Toolkit.Uwp.Notifications;




namespace AgenticAIWPF.Services;





public sealed partial class ToastNotificationsService : IToastNotificationsService
{

    public void ShowToastNotification(ToastNotification toastNotification)
    {
        ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
    }






    public void ShowToastNotification(string title, string message)
    {
        ToastContent content = new()
        {
            Visual = new ToastVisual
            {
                BindingGeneric = new ToastBindingGeneric
                {
                    Children =
                    {
                        new AdaptiveText { Text = title },
                        new AdaptiveText { Text = message }
                    }
                }
            }
        };

        XmlDocument document = new();
        document.LoadXml(content.GetContent());

        ShowToastNotification(new ToastNotification(document));
    }
}