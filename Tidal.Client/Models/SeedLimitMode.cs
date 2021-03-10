namespace Tidal.Client.Models
{
    /// <summary>
    /// Various modes controlling the disposition of a seeding torrent.
    /// </summary>
    public enum SeedLimitMode
    {
        /// <summary>
        /// Reset the torrent's limits to follow the global settings, as set in
        /// <see cref="Session"/>.
        /// </summary>
        FollowGlobalSettings,

        /// <summary>
        /// Apply the limits in <see cref="TorrentMutator.SeedIdleLimitRaw"/> to
        /// this torrent only.
        /// </summary>
        OverrideGlobalSettings,

        /// <summary>
        /// Override the overrides and allow the torrent to run wild, with no
        /// limits, free to become the torrent that it was always meant to be.
        /// </summary>
        Unlimited,
    }
}
