namespace Tidal.Dialogs.ViewModels
{
    /// <summary>
    /// How the dialog lets the view model know how it was closed.
    /// </summary>
    public enum AddTorrentDisposition
    {
        /// <summary>
        /// The user is requesting to add and start the torrent
        /// downloading.
        /// </summary>
        Start,

        /// <summary>
        /// The user is requesting to add, but immediately pause
        /// the torrent, stopping it from downloading just yet.
        /// </summary>
        Pause,

        /// <summary>
        /// The user canceled the dialog.
        /// </summary>
        Cancel,
    }
}
