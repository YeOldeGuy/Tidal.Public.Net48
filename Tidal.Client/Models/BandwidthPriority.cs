namespace Tidal.Client.Models
{
    /// <summary>
    /// Priority levels for downloading torrents. The values specified match
    /// those of the Transmission server (-1..1).
    /// </summary>
    public enum BandwidthPriority
    {
        /// <summary>Low priority</summary>
        Low = -1,

        /// <summary>Normal priority, default value.</summary>
        Normal = 0,

        /// <summary>High priority</summary>
        High = 1,
    }
}
