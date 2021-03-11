using System;

namespace Tidal.Services.Abstract
{
    public interface INotificationService
    {
        void ToastShowInfo(string message, string header = null, TimeSpan timeout = default);

        void ToastShowWarning(string message, string header = null, TimeSpan timeout = default);

        void ShowInfo(string message, string header = null, TimeSpan timeout = default);

        void ShowWarning(string message, string header = null, TimeSpan timeout = default);

        void ShowRetryCancel(string title,
                             string message,
                             string header,
                             Action retryAction,
                             Action cancelAction);

        void ReportPossibleHostFailure(TimeSpan timeout);

        void ReportDownloadComplete(string torrentName);
        void ReportSeedingComplete(string torrentName);
    }
}
