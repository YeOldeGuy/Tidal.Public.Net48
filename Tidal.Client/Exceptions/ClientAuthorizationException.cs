using System;

namespace Tidal.Client.Exceptions
{
    /// <summary>
    /// Represents an error in the basic authentication that the
    /// Transmission host provides. 
    /// </summary>
    public class ClientAuthorizationException : ClientException
    {
        public ClientAuthorizationException()
            : base("Authorization Exception")
        {
        }

        public ClientAuthorizationException(string message)
            : base(message)
        {
        }

        public ClientAuthorizationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
