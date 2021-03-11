using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Tidal.Client.Contracts;

namespace Tidal.Collections
{
    /// <summary>
    ///   A custom <see cref="MergeCollection{T}"/> that presents an
    ///   <see cref="ICollectionView"/>
    ///   to the world for use in the grids that display the data.
    /// </summary>
    /// <remarks>
    ///   In the grid xaml, you should bind to the <see cref="View"/> property
    ///   instead of the "Torrents" or "Peers". Instead, use "Torrents.View" or
    ///   "Peers.View", which will give the grid live sorting capability.
    ///   <para/>
    ///   Remember that the grid always uses a CollectionView to present the
    ///   data, mapping your data into its own view. If you bind to a raw
    ///   collection, then the grid will create a CollectionView for it to use.
    ///   On the other hand, if you give it a CollectionView to bind to, it'll
    ///   use yours, which is where we get the opportunity to do things with
    ///   sorting.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class GridViewCollection<T> : MergeCollection<T>, ILiveSortColumns
        where T : class, INotifyPropertyChanged, IAssignable<T>, ITag, IEquatable<T>
    {
        private readonly CollectionViewSource collectionView;

        public GridViewCollection()
        {
            collectionView = new CollectionViewSource { Source = this };
            collectionView.IsLiveSortingRequested = true;
            foreach (var col in GetSortColumns())
            {
                collectionView.LiveSortingProperties.Add(col);
            }
            View = collectionView.View;
        }


        /// <summary>
        /// Return a collection of the property names that are live-sorted in the grid. 
        /// </summary>
        public virtual IEnumerable<string> GetSortColumns() => new[] { "" };


        /// <summary>
        /// The collection, exposed for use in DataGrids.
        /// <para/>
        /// Ex: <c>ItemsSource={Binding Peers.View}</c>
        /// </summary>
        public ICollectionView View { get; }


        /// <summary>
        ///   This should be called in response to the "Sorting" event of the
        ///   associated grid.
        ///   <para/>
        ///   Ex: <code>Sorting="CodeBehindHandleSorting"</code> in the DataGrid xaml element,
        ///   which then invokes this method:
        ///   <example>
        ///   <code>
        ///   private void CodeBehindHandleSorting(object sender, DataGridSortingEventArgs e)<br/>
        ///   {<br/>
        ///      Peers.HandleSorting(e);<br/>
        ///   }<br/>
        ///   </code>
        ///   </example>
        /// </summary>
        /// <param name="args"></param>
        public virtual void HandleSorting(DataGridSortingEventArgs args)
        {
            args.Handled = true;
            var direction = args.Column.SortDirection != ListSortDirection.Ascending
                ? ListSortDirection.Ascending
                : ListSortDirection.Descending;


            args.Column.SortDirection = direction;

            collectionView.SortDescriptions.Clear();
            collectionView.SortDescriptions.Add(
                new SortDescription(args.Column.SortMemberPath,
                                    args.Column.SortDirection.GetValueOrDefault()));

            collectionView.View.Refresh();
        }
    }
}
