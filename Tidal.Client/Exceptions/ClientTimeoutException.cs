using System;

namespace Tidal.Client.Exceptions
{
    /// <summary>
    /// Represents a timeout error; i.e., the host cannot be connected
    /// to.
    /// </summary>
    public class ClientTimeoutException : ClientException
    {
        public ClientTimeoutException()
            : base("Timeout exception")
        {
        }

        public ClientTimeoutException(string message)
            : base(message)
        {
        }

        public ClientTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
