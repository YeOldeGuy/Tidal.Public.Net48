using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Prism.Commands;
using Tidal.Client.Models;
using Tidal.Collections;
using Tidal.Helpers;
using Tidal.Models.BrokerMessages;
using Tidal.Models.Messages;
using Tidal.Services.Abstract;

namespace Tidal.Controls
{
    /// <summary>
    /// Interaction logic for TorrentGrid.xaml
    /// </summary>
    public partial class TorrentGrid : UserControl
    {
        private readonly IMessenger messenger;
        private bool restoring = false;
        private bool updating = true;

        // Maps the SortMemberPath to a pretty column name
        private readonly Dictionary<string, string> columnsMap = new Dictionary<string, string>()
        {
            { nameof(Torrent.Name), Properties.Resources.TorrentGrid_Friendly_Name },
            { nameof(Torrent.PeersConnected), Properties.Resources.TorrentGrid_Friendly_Peers },
            { nameof(Torrent.UploadRatio), Properties.Resources.TorrentGrid_Friendly_UploadRatio },
            { nameof(Torrent.AverageRateUpload), Properties.Resources.TorrentGrid_Friendly_UploadRate },
            { nameof(Torrent.AverageRateDownload), Properties.Resources.TorrentGrid_Friendly_DownloadRate },
            { nameof(Torrent.TotalSize), Properties.Resources.TorrentGrid_Friendly_Size },
            { nameof(Torrent.ETA), Properties.Resources.TorrentGrid_Friendly_ETA },
            { nameof(Torrent.ActivityDate), Properties.Resources.TorrentGrid_Friendly_Activity },
        };

        public TorrentGrid()
        {
            InitializeComponent();
            messenger = ServiceResolver.Resolve<IMessenger>();

            messenger.Subscribe<RestoreSelectionsMessage>(OnRestore);
            Loaded += (s, e) => { updating = false; };
        }

        private void OnRestore(RestoreSelectionsMessage obj)
        {
            restoring = true;
            try
            {
                SelectTorrents(obj.SelectedHashes);
            }
            finally
            {
                restoring = false;
            }
        }

        public TorrentCollection Torrents
        {
            get { return (TorrentCollection)GetValue(TorrentsProperty); }
            set { SetValue(TorrentsProperty, value); }
        }

        public static readonly DependencyProperty TorrentsProperty =
              DependencyProperty.Register(
                  "Torrents",
                  typeof(TorrentCollection),
                  typeof(TorrentGrid),
                  new PropertyMetadata(null));

        public string Serialize()
        {
            return torrentGrid.Serialize();
        }

        public void Deserialize(string json)
        {
            torrentGrid.Deserialize(json);
        }

        /// <summary>
        /// Use the specified collection of hash strings and select the
        /// corresponding torrents in the grid.
        /// </summary>
        /// <param name="hashes">A collection of torrent hash strings.</param>
        private void SelectTorrents(IEnumerable<string> hashes)
        {
            if (updating || hashes == null || Torrents == null)
                return;

            var selectedTorrents = from t in Torrents
                                   from h in hashes
                                   where t.HashString == h
                                   select t;

            torrentGrid.SelectedItems.Clear();
            foreach (var tor in selectedTorrents)
                torrentGrid.SelectedItems.Add(tor);

            hashes = null;
        }


        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (restoring)
                return;

            // get the hashes for the currently selected items
            IEnumerable<Torrent> tors = torrentGrid?.SelectedItems?.Cast<Torrent>();
            if (tors != null)
                messenger.Send(new SelectionUpdateMessage(tors));
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
                    Header = "Clear Sorting"
                };
                clearItem.Click += ClearSorting;
                menu.Items.Add(clearItem);
                menu.Items.Add(new Separator());

                foreach (var column in torrentGrid.GetHeaderMenuInfo())
                {
                    var header = column.SortMemberPath;
                    columnsMap.TryGetValue(column.SortMemberPath, out header);
                    if (string.IsNullOrEmpty(header))
                        header = column.SortMemberPath;
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
            ICollectionView view = CollectionViewSource.GetDefaultView(torrentGrid.ItemsSource);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                foreach (var column in torrentGrid.Columns)
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


        private DelegateCommand<string> _ToggleTorrentStatus;
        public DelegateCommand<string> ToggleTorrentStatus =>
            _ToggleTorrentStatus = _ToggleTorrentStatus ?? new DelegateCommand<string>((hashString) =>
        {
            var torrent = Torrents.FirstOrDefault(t => t.HashString == hashString);
            if (torrent == null)
                return;
            if (torrent.Status == TorrentStatus.Stopped)
                messenger.Send(new StartTorrentsRequest(new[] { torrent }));
            else
                messenger.Send(new StopTorrentsRequest(new[] { torrent }));
        }, (_) => true);
    }
}
