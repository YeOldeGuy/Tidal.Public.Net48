using System.Threading.Tasks;
using Tidal.Models;

namespace Tidal.Services.Abstract
{
    /// <summary>
    /// Represents a service that listens for messages that want data from the
    /// Transmission host.You don't send the messages directly to this service;
    /// send the requests via the <see cref="Models.BrokerMessages.Messenger"/>.
    /// </summary>
    public interface IBrokerService
    {
        /// <summary>
        /// Open a channel to the client.
        /// </summary>
        /// <param name="ipAddr">An ip address to the client.</param>
        /// <param name="useAuth">If <see langword="true"/>, use basic authentication.</param>
        /// <param name="username">The username for authenticating.</param>
        /// <param name="password">The password for authenticating.</param>
        /// <param name="port">Normall 9091. Adjust if your system is different.</param>
        /// <returns><see langword="true"/> if the open is successful, otherwise <see langword="false"/></returns>
        Task<bool> OpenAsync(string ipAddr,
                             bool useAuth = false,
                             string username = null,
                             string password = null,
                             int port = 9091);

        /// <summary>
        /// Opens a channel to the client
        /// </summary>
        /// <param name="host">A <see cref="Host"/> instance</param>
        /// <returns><see langword="true"/> if the connection is successful</returns>
        Task<bool> OpenAsync(Host host);

        /// <summary>
        /// Direct the service to begin queueing requests, then processing them
        /// in the order received. Responses may not be sent in the same order.
        /// </summary>
        void Start();

        /// <summary>
        /// Direct the service to stop processing requests, emptying any
        /// remaining requests in the queue.
        /// </summary>
        void Stop();
    }
}
