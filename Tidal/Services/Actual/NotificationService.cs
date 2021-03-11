using System;
using Notification.Wpf;
using Prism.Services.Dialogs;
using Tidal.Constants;
using Tidal.Dialogs.ViewModels;
using Tidal.Services.Abstract;

namespace Tidal.Services.Actual
{
    class NotificationService : INotificationService
    {
        private readonly INotificationManager manager;
        private readonly IDialogService dialogService;

        public NotificationService(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            manager = new NotificationManager();
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
            DialogParameters parameters = new DialogParameters()
            {
                { RetryCancelViewModel.TitleParameter, title },
                { RetryCancelViewModel.HeaderParameter, header },
                { RetryCancelViewModel.MessageParameter, message },
            };

            dialogService.ShowDialog(PageKeys.RetryCancel, parameters, r =>
            {
                if (r.Result == ButtonResult.OK)
                    retryAction?.Invoke();
                else if (r.Result == ButtonResult.Cancel)
                    cancelAction?.Invoke();
            }, PageKeys.NoTitleBarWindow);
        }

        public void ReportPossibleHostFailure(TimeSpan timeout)
        {
            //hostFailure = Guid.NewGuid();
            //manager.Show(hostFailure, new NotificationContent
            //{
            //    Message = string.Format(Resources.HostTimeoutMessage_1, timeout.Humanize()),
            //    Title = string.Format(Resources.HostTimeoutHeader_1, DateTime.Now.ToString("G")),
            //}, expirationTime: timeout, areaName: Regions.NotifyArea);
        }

        public void ReportDownloadComplete(string torrentName)
        {
            //await ToastShowInfo(string.Format(Resources.DownloadComplete_1, torrentName), Resources.DownloadComplete);
        }

        public void ReportSeedingComplete(string torrentName)
        {
            //await ToastShowInfo(string.Format(Resources.SeedingComplete_1, torrentName), Resources.SeedingComplete);
        }
    }
}
