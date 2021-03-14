namespace Tidal.ViewModels
{
    /// <summary>
    /// A simple tuple containing a speed to display in the limits menus
    /// along with a flag showing if the speed is already selected.
    /// </summary>
    internal struct SpeedMenuSelector
    {
        /// <summary>
        /// Create a tuple of a speed to be displayed and whether that speed
        /// should be selected, showing a check mark.
        /// </summary>
        /// <param name="speed">A speed in KBps</param>
        /// <param name="selected">
        /// If <see langword="true"/>, then the menu item associated with the
        /// speed should be selected or checked.
        /// </param>
        public SpeedMenuSelector(long speed, bool selected)
        {
            Speed = speed;
            Selected = selected;
        }

        /// <summary>
        /// The speed to put in the menu, specified in KBps
        /// </summary>
        public long Speed { get; }

        /// <summary>
        /// Is this speed the current selected speed? Will have the value
        /// of -1 if the speed is unlimited.
        /// </summary>
        public bool Selected { get; }
    }
}
