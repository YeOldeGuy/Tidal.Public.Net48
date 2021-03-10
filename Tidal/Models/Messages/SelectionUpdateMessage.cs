using System.Collections.Generic;
using System.Linq;
using Tidal.Client.Models;

namespace Tidal.Models.Messages
{
    class SelectionUpdateMessage
    {
        // Why hash strings instead of the Torrents themselves? The
        // instances might not survive long enough. The hash strings
        // survive between sessions.

        public SelectionUpdateMessage(IEnumerable<Torrent> torrents)
        {
            if (torrents != null)
                SelectedHashes = torrents.Select(t => t.HashString).ToList();
        }

        /// <summary>
        /// A possibly empty list of torrent hash strings for the
        /// current set of selected items
        /// </summary>
        public IList<string> SelectedHashes { get; }
    }
}
