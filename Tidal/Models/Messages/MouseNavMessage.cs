namespace Tidal.Models.Messages
{
    /// <summary>
    /// The two directions to move in navigation history.
    /// </summary>
    internal enum MouseNavDirection
    {
        /// <summary>
        /// Move back to previous page in the navigation history.
        /// </summary>
        GoBack,

        /// <summary>
        /// If <see cref="GoBack"/> has been used, this will return to
        /// the page left.
        /// </summary>
        GoForward,
    }

    internal class MouseNavMessage
    {
        /// <summary>
        /// Create a navigation request to move forward or back in the
        /// nav history.
        /// </summary>
        /// <param name="direction"></param>
        public MouseNavMessage(MouseNavDirection direction)
        {
            Direction = direction;
        }

        /// <summary>
        /// Which direction is requested, back or forward.
        /// </summary>
        public MouseNavDirection Direction { get; }
    }
}
