using System.Collections.Generic;
using System.Linq;
using Tidal.Client.Models;

namespace Tidal.Collections
{
    public class FileCollection : GridViewCollection<FileSummary>
    {
        private readonly string[] liveSortColumns =
        {
            // we don't allow sorting on the "Wanted" column
            nameof(FileSummary.Name),
            nameof(FileSummary.Length),
        };

        public override IEnumerable<string> GetSortColumns() => liveSortColumns;

        public void UpdateFromCollection(IEnumerable<Torrent> torrents)
        {
            if (torrents is null)
                return;
            Merge(torrents.SelectMany(t => t.FileSummaries));
        }
    }
}
