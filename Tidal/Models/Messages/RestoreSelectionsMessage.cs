using System.Collections.Generic;
using System.Linq;

namespace Tidal.Models.Messages
{
    /// <summary>
    /// A message to advise subscribers to select the specified torrents
    /// in the display.
    /// </summary>
    internal class RestoreSelectionsMessage
    {
        /// <summary>
        /// Create a message to select the torrents on the grid identified by
        /// the torrent's hashString values.
        /// </summary>
        /// <remarks>
        /// Using the hashStrings assures that the selections will survive a
        /// server restart. ID numbers, what you might expect to use here, are
        /// renumbered at server startup.
        /// </remarks>
        /// <param name="hashes">A list of hashString values to select.</param>
        public RestoreSelectionsMessage(IEnumerable<string> hashes)
        {
            // make sure there's no duplicates and that we own the list
            SelectedHashes = hashes?.Distinct().ToList();
        }

        /// <summary>
        /// The hashString values to select.
        /// </summary>
        public IList<string> SelectedHashes { get; }
    }
}
