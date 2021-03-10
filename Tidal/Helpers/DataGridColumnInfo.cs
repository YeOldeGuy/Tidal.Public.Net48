using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Tidal.Helpers
{
    public class DataGridColumnInfo
    {
        public DataGridColumnInfo() { }

        public DataGridColumnInfo(DataGridColumn col)
        {
            if (col is null)
                return;
            Width = col.Width.ToString();
            SortDirection = col.SortDirection;
            Visibility = col.Visibility;
            DisplayIndex = col.DisplayIndex;
            SortMemberPath = col.SortMemberPath;
        }

        public string Width { get; set; }

        public ListSortDirection? SortDirection { get; set; }

        public Visibility Visibility { get; set; }

        public int DisplayIndex { get; set; }

        public string SortMemberPath { get; set; }
    }
}
