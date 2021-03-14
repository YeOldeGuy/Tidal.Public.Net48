using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using Tidal.Client.Helpers;
using Tidal.Properties;

namespace Tidal.Helpers
{
    public static class DataGridUtils
    {
        public static IEnumerable<DataGridColumn> GetHeaderMenuInfo(this DataGrid grid)
        {
            foreach (var column in grid.Columns)
                yield return column;
        }


        public static void Deserialize(DataGrid grid, string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            var columnInfo = Json.ToObject<IList<DataGridColumnInfo>>(json);
            if (columnInfo == null)
                return;

            if (columnInfo.Count != grid.Columns.Count)
                throw new InvalidOperationException(Resources.DataGridUtils_UnequalColumnCountException);

            var widthConverter = new DataGridLengthConverter();

            //using var _ = grid.Items.DeferRefresh();

            // Clear out any sort descriptions:
            grid.Items.SortDescriptions.Clear();

            for (int i = 0; i < columnInfo.Count; i++)
            {
                grid.Columns[i].Width = (DataGridLength)widthConverter.ConvertFromString(columnInfo[i].Width);
                grid.Columns[i].Visibility = columnInfo[i].Visibility;
                grid.Columns[i].DisplayIndex = columnInfo[i].DisplayIndex;

                // And, if there's a sorted column, add it in right here:
                if (columnInfo[i].SortDirection.HasValue)
                {
                    // Note that this implies that the grid can be sorted by
                    // multiple columns. We don't use that here since I don't
                    // know how to do it through the UI.

                    grid.Columns[i].SortDirection = columnInfo[i].SortDirection;
                    grid.Items.SortDescriptions.Add(new SortDescription(columnInfo[i].SortMemberPath,
                                                                        columnInfo[i].SortDirection.Value));
                }
            }
        }

        /// <summary>
        /// Convert the values necessary for a restore of the grid into
        /// a JSON string.
        /// </summary>
        /// <param name="grid">A Datagrid to persist.</param>
        /// <returns>A JSON representation of the dat grid.</returns>
        public static string Serialize(DataGrid grid)
        {
            var cols = new List<DataGridColumnInfo>();
            foreach (var col in grid.Columns)
            {
                cols.Add(new DataGridColumnInfo(col));
            }
            return Json.ToJSON(cols);
        }
    }
}
