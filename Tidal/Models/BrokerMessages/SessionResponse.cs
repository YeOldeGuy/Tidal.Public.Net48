using Tidal.Client.Models;

namespace Tidal.Models.BrokerMessages
{
    internal class SessionResponse
    {
        /// <summary>
        /// Creates a response that has the <see cref="Session"/> instance
        /// requested by <see cref="SessionRequest"/>.
        /// </summary>
        /// <param name="session">A <see cref="Session"/> instance.</param>
        public SessionResponse(Session session) { Session = session; }

        /// <summary>
        /// Gets the <see cref="Session"/> requested.
        /// </summary>
        public Session Session { get; }
    }
}
