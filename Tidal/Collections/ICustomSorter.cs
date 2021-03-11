using System.Collections;
using System.ComponentModel;

namespace Tidal.Collections
{
    /// <summary>
    /// Defines a special <see cref="IComparer"/> that can have the <see
    /// cref="ListSortDirection"/> specified.
    /// </summary>
    internal interface ICustomSorter : IComparer
    {
        ListSortDirection SortDirection { get; set; }
    }
}
