﻿using Prism.Events;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Tidal.AttachedProperties;
using Tidal.Client.Models;
using Tidal.Collections;
using Tidal.Helpers;
using Tidal.Models.Messages;
using Tidal.Services.Abstract;

namespace Tidal.Controls
{
    public partial class FileGrid : UserControl
    {
        private readonly IMessenger messenger;
        private SubscriptionToken token;

        public FileGrid()
        {
            InitializeComponent();
            messenger = ServiceResolver.Resolve<IMessenger>();
            Loaded += (s, e) =>
                token = messenger.Subscribe<GetSelectedFilesMessage>(GetSelectedFiles, ThreadOption.PublisherThread);
            Unloaded += (s, e) => token?.Dispose();
        }

        private void GetSelectedFiles(GetSelectedFilesMessage getSelectionMessage)
        {
            // Gets the selected FileSummary instances,
            var files = fileGrid.SelectedItems.Cast<FileSummary>().ToList();

            // assigns them to the message so the caller can see,
            getSelectionMessage.SelectedFiles = files;

            // then, toggles the status so that the display needn't wait until
            // the next torrent update.
            foreach (var f in files)
                f.Wanted = getSelectionMessage.Wanted;
        }

        public void Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return;
            fileGrid.Deserialize(json);
            DataGridUtils.FixStarColumn(fileGrid, nameof(FileSummary.Name));
        }

        public string Serialize()
        {
            return fileGrid.Serialize();
        }

        public FileCollection Files
        {
            get => (FileCollection)GetValue(FilesProperty);
            set => SetValue(FilesProperty, value);
        }
        public static readonly DependencyProperty FilesProperty =
            DependencyProperty.Register("Files",
                                        typeof(FileCollection),
                                        typeof(FileGrid),
                                        new PropertyMetadata(null));

        private void HandleSorting(object sender, DataGridSortingEventArgs e)
        {
            Files.HandleSorting(e);
        }

        private void ColumnHeaderMenuOpened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu menu)
            {
                if (menu.Items.Count <= 0)
                    return;

                if (menu.Items[0] != null)
                    ((MenuItem)menu.Items[0]).Click -= ClearSorting;

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

                foreach (var column in fileGrid.GetHeaderMenuInfo())
                {
                    var header = PropertyHelpers.GetDescription<FileSummary>(SortMember.GetName(column));
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
            ICollectionView view = CollectionViewSource.GetDefaultView(fileGrid.ItemsSource);
            if (view != null)
            {
                view.SortDescriptions.Clear();
                foreach (var column in fileGrid.Columns)
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
