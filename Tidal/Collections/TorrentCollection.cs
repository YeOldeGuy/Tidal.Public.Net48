using System.Collections.Generic;
using Tidal.Client.Models;

namespace Tidal.Collections
{
    /// <summary>
    /// A specialized <see cref="GridViewCollection{T}"/> for
    /// <see cref="Torrent"/>s.
    /// </summary>
    public class TorrentCollection : GridViewCollection<Torrent>
    {
        private readonly string[] liveSortColumns =
        {
            nameof(Torrent.Name),
            nameof(Torrent.PeersConnected),
            nameof(Torrent.UploadRatio),
            nameof(Torrent.AverageRateUpload),
            nameof(Torrent.AverageRateDownload),
            nameof(Torrent.ETA),
            nameof(Torrent.TotalSize),
            nameof(Torrent.ActivityDate),
        };

        /// <summary>
        /// Create a new <see cref="TorrentCollection"/>.
        /// </summary>
        public TorrentCollection()
        {
        }

        public override IEnumerable<string> GetSortColumns() => liveSortColumns;
    }
}
