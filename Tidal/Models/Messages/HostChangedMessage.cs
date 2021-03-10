using System;

namespace Tidal.Models.Messages
{
    /// <summary>
    /// When the user chooses a new host to monitor, send out
    /// this message to any subscribers.
    /// </summary>
    internal class HostChangedMessage
    {
        /// <summary>
        /// Create a broadcast message that the user has selected
        /// the <see cref="Models.Host"/> to poll.
        /// </summary>
        /// <param name="id">The <see cref="Models.Host.Id"/> of the selected host.</param>
        public HostChangedMessage(Guid id)
        {
            ActiveId = id;
        }

        /// <summary>
        /// The Id of the selected host.
        /// </summary>
        public Guid ActiveId { get; }
    }
}
