using System;

namespace Tidal.Models.Messages
{
    /// <summary>
    /// The base class for all of the various error messages that the
    /// <see cref="Services.Abstract.IBrokerService"/> can emit.
    /// </summary>

    internal abstract class ErrorMessage
    {
        public ErrorMessage(string message, string header = null, TimeSpan timeout = default)
        {
            Message = message;
            Header = header;
            Timeout = timeout;
        }

        public string Message { get; }

        public string Header { get; }

        public TimeSpan Timeout { get; }
    }


    /// <summary>
    /// Represents an error that cannot be recovered from without
    /// intervention, such as a lost connection to the client.
    /// </summary>
    internal class FatalMessage : ErrorMessage
    {
        /// <summary>
        /// Create a message for an unrecoverable error.
        /// </summary>
        /// <param name="message">The message itself</param>
        /// <param name="header">A brief synopsis of the error.</param>
        public FatalMessage(string message, string header = default)
            : base(message, header)
        {
        }
    }


    /// <summary>
    /// A type of error message that isn't fatal but isn't exactly just
    /// informational, either. Like an unable to parse torrent file error.
    /// </summary>
    internal class WarningMessage : ErrorMessage
    {
        /// <summary>
        /// Create a message to inform the user of an error that isn't fatal.
        /// </summary>
        public WarningMessage(string message, string header = null, TimeSpan timeout = default)
            : base(message, header, timeout)
        {
        }
    }

    internal class InfoMessage : ErrorMessage
    {
        /// <summary>
        /// Create a message to inform the user of an error that isn't fatal.
        /// </summary>
        public InfoMessage(string message, string header = null, TimeSpan timeout = default)
            : base(message, header, timeout)
        {
        }
    }
}
