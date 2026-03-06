// 2026/03/05
//  Solution: RAGDataIngestionWPF
//  Project:   RAGDataIngestionWPF
//  File:         ToastNotificationsService.Samples.cs
//   Author: Kyle L. Crowder



using Microsoft.Toolkit.Uwp.Notifications;

using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;




namespace RAGDataIngestionWPF.Services;





public partial class ToastNotificationsService
{
    public void ShowToastNotificationSample()
    {
        // Create the toast content
        ToastContent content = new()
        {
            // More about the Launch property at https://docs.microsoft.com/dotnet/api/communitytoolkit.winui.notifications.toastcontent
            Launch = "ToastContentActivationParams",

            Visual = new ToastVisual
            {
                BindingGeneric = new ToastBindingGeneric
                {
                    Children =
                                {
                                        new AdaptiveText
                                        {
                                                Text = "Sample Toast Notification"
                                        },

                                        new AdaptiveText
                                        {
                                                Text = @"Click OK to see how activation from a toast notification can be handled in the ToastNotificationService."
                                        }
                                }
                }
            },

            Actions = new ToastActionsCustom
            {
                Buttons =
                        {
                                // More about Toast Buttons at https://docs.microsoft.com/dotnet/api/communitytoolkit.winui.notifications.toastbutton
                                new ToastButton("OK", "ToastButtonActivationArguments")
                                {
                                        ActivationType = ToastActivationType.Foreground
                                },

                                new ToastButtonDismiss("Cancel")
                        }
            }
        };

        // Add the content to the toast
        XmlDocument doc = new();
        doc.LoadXml(content.GetContent());
        ToastNotification toast = new(doc)
        {
            // TODO: Set a unique identifier for this notification within the notification group. (optional)
            // More details at https://docs.microsoft.com/uwp/api/windows.ui.notifications.toastnotification.tag
            Tag = "ToastTag"
        };

        // And show the toast
        ShowToastNotification(toast);
    }
}