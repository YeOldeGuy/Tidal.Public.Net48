using System.Collections.Generic;

namespace Tidal.Collections
{
    /// <summary>
    /// Defines a contract to get the column names of a data grid that should be
    /// live-sorted.
    /// </summary>
    public interface ILiveSortColumns
    {
        /// <summary>
        /// Get the names of columns (SortMemberPath) of the DataGrid that
        /// should be live-sorted.
        /// </summary>
        /// <returns>A list of column names.</returns>
        IEnumerable<string> GetSortColumns();
    }
}
