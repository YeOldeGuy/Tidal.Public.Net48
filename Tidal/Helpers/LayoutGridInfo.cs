using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace Tidal.Helpers
{
    public class LayoutGridInfo
    {
        /// <summary>
        /// Empty constructor for Json methods.
        /// </summary>
        public LayoutGridInfo() { }

        /// <summary>
        /// Build a new <see cref="LayoutGridInfo"/> from the data in the <see cref="RowDefinition"/>.
        /// </summary>
        /// <param name="rowDef">Use the <see cref="RowDefinition.Height"/> data.</param>
        public LayoutGridInfo(RowDefinition rowDef)
        {
            if (rowDef == null)
                throw new ArgumentException("RowDefinition is null in LayoutGridInfo");

            UnitType = rowDef.Height.GridUnitType;
            Length = rowDef.Height.Value;
        }

        /// <summary>
        /// Build a new <see cref="LayoutGridInfo"/> from the data in the <see cref="ColumnDefinition"/>.
        /// </summary>
        /// <param name="colDef">Use the <see cref="ColumnDefinition.Width"/> data.</param>
        public LayoutGridInfo(ColumnDefinition colDef)
        {
            if (colDef == null)
                throw new ArgumentException("ColumnDefinition is null in LayoutGridInfo");

            UnitType = colDef.Width.GridUnitType;
            Length = colDef.Width.Value;
        }


        /// <summary>
        /// One of the types from <see cref="Windows.UI.Xaml.GridUnitType"/>.
        /// </summary>
        public GridUnitType UnitType { get; set; }

        /// <summary>
        /// Width or height of the grid element.
        /// </summary>
        public double Length { get; set; }


        [IgnoreDataMember]
        public RowDefinition RowDefinition => new RowDefinition() { Height = new GridLength(Length, UnitType) };


        [IgnoreDataMember]
        public ColumnDefinition ColumnDefinition => new ColumnDefinition() { Width = new GridLength(Length, UnitType) };
    }
}
