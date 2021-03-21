﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

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
            var col = grid.Columns.Where(c => c.SortMemberPath == starColumn).FirstOrDefault();
            var width = col.Width.Value;
            if (col != null)
                col.Width = new DataGridLength(width, DataGridLengthUnitType.Star);
        }
    }
}
