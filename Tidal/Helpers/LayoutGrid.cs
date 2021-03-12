﻿using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Tidal.Helpers
{
    /// <summary>
    /// Holds enough data from a <see cref="Grid"/> to allow restoring
    /// it to a previous state.
    /// </summary>
    public class LayoutGrid
    {
        public LayoutGrid() { }

        /// <summary>
        /// Create a container with <see cref="RowDefinitionCollection"/> in it.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="rowDefs"></param>
        public LayoutGrid(string name, RowDefinitionCollection rowDefs)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Need name in LayoutGrid cons");
            if (rowDefs == null)
                throw new ArgumentException("Row definitions is null in LayoutGrid constructor");

            GridName = name;
            LayoutSpecs = new List<LayoutGridInfo>();
            foreach (var def in rowDefs)
            {
                LayoutSpecs.Add(new LayoutGridInfo(def));
            }
        }

        /// <summary>
        /// Create a container with a <see cref="ColumnDefinitionCollection"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="colDefs"></param>
        public LayoutGrid(string name, ColumnDefinitionCollection colDefs)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Need name in LayoutGrid constructor");
            if (colDefs == null)
                throw new ArgumentException("Column definitions is null in LayoutGrid constructor");

            GridName = name;
            LayoutSpecs = new List<LayoutGridInfo>();
            foreach (var def in colDefs)
            {
                LayoutSpecs.Add(new LayoutGridInfo(def));
            }
        }

        /// <summary>
        /// The name of the grid as defined in the XAML code, used to
        /// restore information.
        /// </summary>
        public string GridName { get; set; }

        public IList<LayoutGridInfo> LayoutSpecs { get; set; }
    }
}
