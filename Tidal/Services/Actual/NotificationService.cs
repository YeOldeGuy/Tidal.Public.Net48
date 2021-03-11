using System;
using Humanizer;
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
            manager.Show(new NotificationContent
            {
                Message = message,
                Title = header ?? "Info",
                Type = NotificationType.Information,
            }, expirationTime: timeout == default ? TimeSpan.MaxValue : timeout);
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
            manager.Show(new NotificationContent
            {
                Message = string.Format(Resources.HostTimeoutMessage_1, timeout.Humanize()),
                Title = string.Format(Resources.HostTimeoutHeader_1, DateTime.Now.ToString("G")),
            }, expirationTime: timeout, areaName: Regions.NotifyArea);
        }

        public void ReportDownloadComplete(string torrentName)
        {
            manager.Show(new NotificationContent
            {
                Title = Resources.DownloadComplete,
                Message = torrentName,
                Type = NotificationType.Success,
            }, expirationTime: TimeSpan.MaxValue);
        }

        public void ReportSeedingComplete(string torrentName)
        {
            manager.Show(new NotificationContent
            {
                Message = string.Format(Resources.SeedingComplete_1, torrentName),
                Title = Resources.SeedingComplete,
                Type = NotificationType.Success,
            }, expirationTime: TimeSpan.MaxValue);
        }
    }
}
