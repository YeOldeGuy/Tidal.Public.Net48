using System;

namespace Tidal.Services.Abstract
{
    public interface INotificationService
    {
        /// <summary>
        /// Show a toast message (Windows10 style) with the specified message
        /// and a dismiss button.
        /// </summary>
        /// <remarks>
        /// Note that the timeout is heeded, but dismissing a toast from the
        /// notification center without giving the user a chance to see and act
        /// on it would be considered bad form.
        /// </remarks>
        /// <param name="message">The message text</param>
        /// <param name="header">The header text</param>
        /// <param name="timeout">Time after which the toast is removed.</param>
        void ToastShowInfo(string message, string header = null, TimeSpan timeout = default);

        /// <summary>
        /// Displays a toast message, ala Windows 10, warning the user of
        /// impending doom.
        /// </summary>
        /// <param name="message">The message text</param>
        /// <param name="header">The header text</param>
        /// <param name="timeout">Time after which the toast is removed.</param>
        void ToastShowWarning(string message, string header = null, TimeSpan timeout = default);

        /// <summary>
        /// Displays an in-app message.
        /// </summary>
        /// <param name="message">The message text</param>
        /// <param name="header">The header text</param>
        /// <param name="timeout">Time after which the message popup is removed.</param>
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
