using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Tidal.AttachedProperties;

namespace Tidal.Helpers
{
    public static class DataGridUtils
    {
        public static IEnumerable<DataGridColumn> GetHeaderMenuInfo(this DataGrid grid)
        {
            foreach (var column in grid.Columns)
                yield return column;
        }

        /// <summary>
        /// Make sure that the specified grid column is of "Star" width, no
        /// matter what was stored at persist time.
        /// </summary>
        /// <param name="grid">The <see cref="DataGrid"/> the column is in.</param>
        /// <param name="starColumn">The SortMemberPath of the column.</param>
        public static void FixStarColumn(DataGrid grid, string starColumn)
        {
            var col = grid.Columns.FirstOrDefault(c => c.SortMemberPath == starColumn);
            if (col == null)
            {
                col = grid.Columns.FirstOrDefault(c => SortMember.GetName(c) == starColumn);
            }
            if (col != null)
            {
                var width = col.Width.Value;
                col.Width = new DataGridLength(width, DataGridLengthUnitType.Star);
            }
        }
    }
}
