using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Controls;
using Tidal.Properties;

namespace Tidal.Helpers
{
    /// <summary>
    /// Holds just enough information about a XAML Grid layout to be
    /// able to restore it, namely just the row and column definitions.
    /// </summary>
    public class LayoutInfo
    {
        /// <summary>
        /// The list of layout grids and their sizing.
        /// </summary>
        [DataMember]
        public IList<LayoutGrid> LayoutGrids { get; set; }

        /// <summary>
        /// Create an instance of <see cref="LayoutInfo"/>, which can
        /// hold a number of <see cref="LayoutGrid"/>s.
        /// </summary>
        public LayoutInfo()
        {
            LayoutGrids = new List<LayoutGrid>();
        }

        /// <summary>
        /// Add a layout consisting of row definitions
        /// </summary>
        /// <param name="layoutGridName"></param>
        /// <param name="rows"></param>
        public void AddLayout(string layoutGridName, RowDefinitionCollection rows)
        {
            if (string.IsNullOrEmpty(layoutGridName))
                throw new ArgumentNullException(Resources.LayoutInfo_GridNameException);
            if (rows == null)
                throw new ArgumentNullException(Resources.LayoutInfo_DefinitionNullException);

            LayoutGrids.Add(new LayoutGrid(layoutGridName, rows));
        }

        /// <summary>
        /// Add a layout of column definitions.
        /// </summary>
        /// <param name="layoutGridName"></param>
        /// <param name="cols"></param>
        public void AddLayout(string layoutGridName, ColumnDefinitionCollection cols)
        {
            if (string.IsNullOrEmpty(layoutGridName))
                throw new ArgumentNullException(Resources.LayoutInfo_GridNameException);
            if (cols == null)
                throw new ArgumentNullException(Resources.LayoutInfo_DefinitionNullException);

            LayoutGrids.Add(new LayoutGrid(layoutGridName, cols));
        }

        public LayoutGrid GetLayoutGrid(string layoutGridName)
        {
            if (string.IsNullOrEmpty(layoutGridName))
                throw new ArgumentNullException(Resources.LayoutInfo_GridNameException);

            return LayoutGrids.FirstOrDefault(g => g.GridName.ToLower() == layoutGridName.ToLower());
        }
    }
}
