using System;

namespace Tidal.Client.Exceptions
{
    /// <summary>
    /// Represents errors that occur in communicating with the transmission
    /// client. Note that all exceptions from the client are mapped to this or
    /// one of its derivatives, including COM errors, HTTP errors, etc.
    /// </summary>
    public class ClientException : Exception
    {
        public ClientException()
            : base("Unknown client exception")
        {
        }

        public ClientException(string message)
            : base(message)
        {
        }

        public ClientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
