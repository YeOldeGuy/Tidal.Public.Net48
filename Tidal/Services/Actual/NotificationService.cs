using System;
using Humanizer;
using Microsoft.Toolkit.Uwp.Notifications;
using Notification.Wpf;
using Tidal.Constants;
using Tidal.Properties;
using Tidal.Services.Abstract;

namespace Tidal.Services.Actual
{
    class NotificationService : INotificationService
    {
        private readonly INotificationManager manager;

        public NotificationService()
        {
            manager = manager ?? new NotificationManager();
        }

        public void ToastShowInfo(string message, string header = null, TimeSpan timeout = default)
        {
            new ToastContentBuilder()
                .AddText(header)
                .AddText(message)
                .AddButton(new ToastButton().SetDismissActivation())
                .SetToastDuration(ToastDuration.Short)
                .Show();
        }

        public void ShowInfo(string message, string header = null, TimeSpan timeout = default)
        {
            manager.Show(new NotificationContent
            {
                Message = message,
                Title = header ?? "Info",
                Type = NotificationType.Information,
            }, expirationTime: timeout == default ? TimeSpan.MaxValue : timeout, areaName: Regions.NotifyArea);
        }

        public void ToastShowWarning(string message, string header = null, TimeSpan timeout = default)
        {
            manager.Show(new NotificationContent
            {
                Message = message,
                Title = header ?? "Warning",
                Type = NotificationType.Warning,
            }, expirationTime: timeout == default ? TimeSpan.MaxValue : timeout);
        }

        public void ShowWarning(string message, string header = null, TimeSpan timeout = default)
        {
            manager.Show(new NotificationContent
            {
                Message = message,
                Title = header ?? "Warning",
                Type = NotificationType.Warning,
            }, expirationTime: timeout == default ? TimeSpan.MaxValue : timeout, areaName: Regions.NotifyArea);
        }

        public void ShowRetryCancel(string title, string message, string header,
                                    Action retryAction, Action cancelAction)
        {
            manager.Show(new NotificationContent
            {
                LeftButtonAction = retryAction,
                RightButtonAction = cancelAction,
                LeftButtonContent = "Retry",
                RightButtonContent = "Cancel",
                Title = header,
                Message = message,
                Type = NotificationType.Error,
            }, expirationTime: TimeSpan.MaxValue, areaName: Regions.NotifyArea);
        }

        public void ReportPossibleHostFailure(TimeSpan timeout)
        {
            new ToastContentBuilder()
                .AddText(string.Format(Resources.HostTimeoutHeader_1, DateTime.Now.ToString("G")), AdaptiveTextStyle.Title)
                .AddText(string.Format(Resources.HostTimeoutMessage_1, timeout.Humanize()))
                .AddButton(new ToastButton().SetDismissActivation())
                .Show();
        }

        public void ReportDownloadComplete(string torrentName)
        {
            new ToastContentBuilder()
                .AddText(Resources.DownloadComplete, AdaptiveTextStyle.Header)
                .AddText(torrentName, AdaptiveTextStyle.Body)
                .AddButton(new ToastButton().SetDismissActivation())
                .SetToastDuration(ToastDuration.Long)
                .Show();
        }

        public void ReportSeedingComplete(string torrentName)
        {
            new ToastContentBuilder()
                .AddText(Resources.SeedingComplete, AdaptiveTextStyle.Title)
                .AddText(string.Format(Resources.SeedingComplete_1, torrentName), AdaptiveTextStyle.Body)
                .AddButton(new ToastButton().SetDismissActivation())
                .SetToastDuration(ToastDuration.Long)
                .Show();
        }
    }
}
