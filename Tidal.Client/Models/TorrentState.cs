using System;

namespace Tidal.Client.Models
{
    /// <summary>
    /// The various states a torrent can be in. This is different
    /// than the <see cref="TorrentStatus"/>, and is determined
    /// by the code by looking at various parameters.
    /// </summary>
    [Flags]
    public enum TorrentState
    {
        /// <summary>
        /// Torrent is currently downloading.
        /// </summary>
        Downloading = 1 << 0,

        /// <summary>
        /// Torrent is finished. A torrent is only considered completed
        /// if it has met its seed limit, thus only a stopped and finished
        /// torrent can be considered Completed.
        /// </summary>
        Completed = 1 << 1,

        /// <summary>
        /// Torrent is downloading or seeding.
        /// </summary>
        Active = 1 << 2,

        /// <summary>
        /// Torrent is not downloading or seeding, but is set to
        /// do so.
        /// </summary>
        Inactive = 1 << 3,

        /// <summary>
        /// Torrent has stopped, either through user action, or
        /// hitting a seeding limit.
        /// </summary>
        Stopped = 1 << 4,

        /// <summary>
        /// Paused, which is defined as stopped, but not done downloading.
        /// </summary>
        Paused = 1 << 5,

        /// <summary>
        /// Checking files or torrent.
        /// </summary>
        Checking = 1 << 6,

        /// <summary>
        /// Queued up, but not active since too many other are already.
        /// </summary>
        Waiting = 1 << 7,

        /// <summary>
        /// Is currently seeding, but has no peers either attached or leeching.
        /// </summary>
        Seeding = 1 << 8,

        /// <summary>
        /// The torrent has an error, usually that the files have 
        /// disappeared, or a hash check failed.
        /// </summary>
        Error = 1 << 9,

        /// <summary>
        /// ALL of the torrents, no matter their state.
        /// </summary>
        All = (Downloading | Completed | Active | Inactive | Stopped | Paused | Error | Checking | Seeding | Waiting)
    }
}
