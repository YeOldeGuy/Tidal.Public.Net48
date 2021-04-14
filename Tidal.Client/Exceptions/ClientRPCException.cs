using System;

namespace Tidal.Client.Exceptions
{
    /// <summary>
    /// An exception thrown when the RPC fails for some reason,
    /// usually a malformed torrent file.
    /// </summary>
    public class ClientRPCException : ClientException
    {
        public string Header
        {
            get;
        }

        public ClientRPCException()
            : base("Remote Procedure Call exception")
        {
        }

        public ClientRPCException(string message)
            : base(message)
        {
        }

        public ClientRPCException(string message, string header)
            : base(message)
        {
            Header = header;
        }

        public ClientRPCException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
