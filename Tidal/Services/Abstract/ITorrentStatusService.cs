using System.Collections.Generic;
using Tidal.Client.Models;

namespace Tidal.Services.Abstract
{
    public interface ITorrentStatusService
    {
        void CheckForConnection(IEnumerable<Torrent> torrents);

        void CheckStatus(IEnumerable<Torrent> torrents);
    }
}
