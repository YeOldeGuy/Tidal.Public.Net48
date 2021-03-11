using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Tidal.Client.Models;
using Tidal.Comparers;
using Tidal.Helpers;
using Tidal.Properties;
using Tidal.Services.Abstract;

namespace Tidal.Collections
{
    public class PeerCollection : GridViewCollection<Peer>
    {
        private readonly IGeoService geoService;
        private readonly string[] liveSortColumns =
        {
            nameof(Peer.Address),
            nameof(Peer.Location),
            nameof(Peer.AverageToClient),
            nameof(Peer.AverageToPeer),
        };

        public override IEnumerable<string> GetSortColumns() => liveSortColumns;


        public PeerCollection()
            : this(ServiceResolver.Resolve<IGeoService>())
        {
        }

        public PeerCollection(IGeoService geoService)
        {
            this.geoService = geoService;
        }

        public void UpdateFromCollection(IEnumerable<Torrent> torrents)
        {
            if (torrents == null)
                return;

            Merge(torrents.SelectMany(t => t.Peers));
        }

        protected override async void InsertItem(int index, Peer item)
        {
            base.InsertItem(index, item);

            if (geoService != null)
            {
                var location = await geoService.GetRawLocationAsync(item.Address);
                item.Location = geoService.GetFullLocation(location);
            }
        }

        protected override void AfterMerge(IEnumerable<Peer> removals)
        {
            base.AfterMerge(removals);

            // After starting, if the geo database isn't ready yet, then the
            // locations will be invalid. Since this method is called at every
            // update, then this is a good time to try and get the location
            // again.
            //
            try
            {
                foreach (var peer in this)
                {
                    if (!peer.LocationValid && geoService != null)
                    {
                        var location = geoService.GetRawLocation(peer.Address);
                        if (location != null)
                        {
                            peer.LocationValid = true;
                            peer.Geo = location;
                            peer.Location = geoService.GetFullLocation(location);
                        }
                        else
                        {
                            peer.Location = Resources.PeerCollection_NoLocation;
                            peer.LocationValid = false;
                            peer.Geo = null;
                        }
                    }
                }
            }
            catch (InvalidOperationException) { }
        }


        private readonly ICustomSorter ipComparer = new IpComparer();
        private readonly ICustomSorter locationComparer = new LocationComparer();

        public override void HandleSorting(DataGridSortingEventArgs e)
        {
            if (e.Column.SortMemberPath == nameof(Peer.Address) || e.Column.SortMemberPath == nameof(Peer.Location))
            {
                e.Handled = true;
                var direction = e.Column.SortDirection != ListSortDirection.Ascending
                    ? ListSortDirection.Ascending
                    : ListSortDirection.Descending;


                e.Column.SortDirection = direction;
                if (e.Column.SortMemberPath == nameof(Peer.Address))
                {
                    ipComparer.SortDirection = direction;
                    var view = (ListCollectionView)View;
                    view.CustomSort = ipComparer;
                }
                else if (e.Column.SortMemberPath == nameof(Peer.Location))
                {
                    locationComparer.SortDirection = direction;
                    var view = (ListCollectionView)View;
                    view.CustomSort = locationComparer;
                }
            }
            else
                base.HandleSorting(e);
        }
    }
}
