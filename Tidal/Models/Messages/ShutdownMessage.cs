namespace Tidal.Models.Messages
{
    /// <summary>
    /// A request notifying subscribers that the app is going
    /// to shutdown.
    /// </summary>
    internal class ShutdownMessage
    {
        /// <summary>
        /// If any of the subscribers to this message set this value to <see
        /// langword="true"/>, then the shutdown will be canceled, if possible.
        /// </summary>
        public bool Cancel { get; set; } = false;
    }
}
