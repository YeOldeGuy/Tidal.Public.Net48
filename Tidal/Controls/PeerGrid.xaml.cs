using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Tidal.Client.Models;
using Tidal.Collections;
using Tidal.Helpers;

namespace Tidal.Controls
{
    public partial class PeerGrid : UserControl
    {
        public PeerGrid()
        {
            InitializeComponent();
            Loaded += (s, x) => { AttachCustomSorters(); };
        }

        public PeerCollection Peers
        {
            get { return (PeerCollection)GetValue(PeersProperty); }
            set { SetValue(PeersProperty, value); }
        }
        public static readonly DependencyProperty PeersProperty =
            DependencyProperty.Register(nameof(Peers),
                                        typeof(PeerCollection),
                                        typeof(PeerGrid),
                                        new PropertyMetadata(null));

        public void Deserialize(string json)
        {
            peerGrid.Deserialize(json);

            DataGridUtils.FixStarColumn(peerGrid, nameof(Peer.Location));
        }

        public string Serialize()
        {
            return peerGrid.Serialize();
        }

        private void AttachCustomSorters()
        {
            CheckSortDirection("Address");
            CheckSortDirection("Location");
        }


        private void CheckSortDirection(string sortMemberPath)
        {
            // We're going to force the grid to perform the sort, if needed. So,
            // if the column is sorted -- i.e., SortDirection isn't null -- we
            // need to reverse the current value of the column's direction. Why?
            // Because when we call the HandleSorting method, it will reverse it
            // again (like clicking on the header).

            var col = peerGrid.Columns.Where(c => c.SortMemberPath == sortMemberPath).FirstOrDefault();
            if (col != null && col.SortDirection.HasValue)
            {
                col.SortDirection = col.SortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
                HandleSorting(peerGrid, new DataGridSortingEventArgs(col));
            }
        }

        private void HandleSorting(object sender, DataGridSortingEventArgs e)
        {
            Peers.HandleSorting(e);
        }

        private void ColumnHeaderMenuOpened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
            {
                if (menu.Items.Count <= 0)
                    return;

                (menu.Items[0] as MenuItem).Click -= ClearSorting;

                foreach (var item in menu.Items)
                {
                    if (item is MenuItem menuItem)
                        menuItem.Click -= HeaderMenuItemClicked;
                }
                menu.Items.Clear();
                var clearItem = new MenuItem
                {
                    Header = Properties.Resources.ClearSorting
                };
                clearItem.Click += ClearSorting;
                menu.Items.Add(clearItem);
                menu.Items.Add(new Separator());

                foreach (var column in peerGrid.GetHeaderMenuInfo())
                {
                    var header = PropertyHelpers.GetDescription<Peer>(column.SortMemberPath);
                    var item = new MenuItem
                    {
                        Header = header,
                        IsChecked = column.Visibility == Visibility.Visible,
                        Tag = column,
                    };
                    item.Click += HeaderMenuItemClicked;
                    menu.Items.Add(item);
                }
            }
        }

        private void ClearSorting(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(peerGrid.ItemsSource);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                foreach (var column in peerGrid.Columns)
                {
                    column.SortDirection = null;
                }
            }
        }

        private void HeaderMenuItemClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                item.IsChecked = !item.IsChecked;
                if (item.Tag is DataGridColumn column)
                {
                    column.Visibility = item.IsChecked ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }
    }
}
