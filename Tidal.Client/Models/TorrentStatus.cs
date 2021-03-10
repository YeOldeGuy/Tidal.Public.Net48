using System.ComponentModel;

namespace Tidal.Client.Models
{
    public enum TorrentStatus
    {
        /// <summary>
        /// Torrent is currently stopped, either through reaching a limit
        /// or through user action.
        /// </summary>
        Stopped = 0,

        /// <summary>
        /// Server is currently queued up to check the files.
        /// </summary>
        [Description("Queued for checking")]
        CheckWait = 1,

        /// <summary>
        /// Checking files.
        /// </summary>
        [Description("Checking Files")]
        Checking = 2,           /* Checking files */

        /// <summary>
        /// Torrent is currently queued up, waiting for another to finish.
        /// </summary>
        [Description("Waiting to download")]
        Queued = 3,             /* Queued to download */

        /// <summary>
        /// Torrent is being downloaded.
        /// </summary>
        Downloading = 4,        /* Downloading */

        /// <summary>
        /// Torrent is currently awaiting for the queue to empty before
        /// being seeded.
        /// </summary>
        [Description("Waiting to Seed")]
        QueuedToSeed = 5,       /* Queued to seed */

        /// <summary>
        /// Torrent is currently being seeded.
        /// </summary>
        Seeding = 6             /* Seeding */
    }
}
