using System;

namespace Tidal.Models.Messages
{
    internal class StatusInfoMessage
    {
        public StatusInfoMessage(string message, TimeSpan timeout = default)
        {
            Message = message;
            Timeout = timeout;
        }

        public string Message { get; }

        public TimeSpan Timeout { get; }
    }
}
