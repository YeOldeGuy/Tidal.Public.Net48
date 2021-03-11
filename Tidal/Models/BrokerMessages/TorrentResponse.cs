using System.Collections.Generic;
using Tidal.Client.Models;

namespace Tidal.Models.BrokerMessages
{
    internal class TorrentResponse
    {
        public TorrentResponse(IEnumerable<Torrent> torrents)
        {
            Torrents = torrents;
        }

        public IEnumerable<Torrent> Torrents { get; }
    }
}
