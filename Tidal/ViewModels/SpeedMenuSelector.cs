namespace Tidal.ViewModels
{
    internal struct SpeedMenuSelector
    {
        public SpeedMenuSelector(long speed, bool selected)
        {
            Speed = speed;
            Selected = selected;
        }
        public long Speed { get; }
        public bool Selected { get; }
    }
}
