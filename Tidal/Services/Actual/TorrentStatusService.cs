using System;
using System.Collections.Generic;
using System.Linq;
using Tidal.Client.Models;
using Tidal.Services.Abstract;

namespace Tidal.Services.Actual
{
    internal class TorrentStatusService : ITorrentStatusService
    {
        private readonly Dictionary<int, TorrentStatus> statusMap;
        private readonly INotificationService notificationService;
        private readonly ISettingsService settingsService;
        private readonly double MinimumSeeding = 1.0;

        private DateTime lastUpdatedPeers;

        public TorrentStatusService(INotificationService notificationService,
                                    ISettingsService settingsService)
        {
            statusMap = new Dictionary<int, TorrentStatus>();
            this.notificationService = notificationService;
            this.settingsService = settingsService;
            lastUpdatedPeers = DateTime.Now;
        }

        public void CheckForConnection(IEnumerable<Torrent> torrents)
        {
            if (torrents == null || !torrents.Any() || torrents.All(t => t.Status == TorrentStatus.Stopped))
            {
                return;
            }

            // See if any peers are attached. Not having peers is fine, but also
            // a sign that the server may have lost its interface to the outside
            // world. This is a problem on my current setup with OpenVPN talking
            // to PIA -- it just drops once in a while, usually after an "apt
            // upgrade". I have the network firewall set up to close the
            // interface if that happens so that nothing leaks out -- it's
            // stupid to have a VPN and then let all the apps keep talking if
            // that VPN dies. Transmission has no idea; it just figures not much
            // is happening.

            // So, if no peers have been seen in the time specified in the
            // settings (normally 1 hour), report that to the user.

            if (torrents.Any(t => t.PeersConnected > 0))
            {
                lastUpdatedPeers = DateTime.Now;
            }
            else if (DateTime.Now - lastUpdatedPeers > settingsService.DeadHostTime)
            {
                notificationService.ReportPossibleHostFailure(DateTime.Now - lastUpdatedPeers);
                lastUpdatedPeers = DateTime.MaxValue;
            }
        }

        private IEnumerable<int> FindRemovals(IEnumerable<Torrent> freshtorrents)
        {
            // Find the torrents that the status map has that are no longer in
            // the torrents being maintained by the client. Remove them.
            //
            // Return a separate list or suffer the dread "collection modified"
            // exception.
            //
            return statusMap.Keys.Except(freshtorrents.Select(t => t.Id)).ToList();
        }

        public void CheckStatus(IEnumerable<Torrent> torrents)
        {
            if (torrents == null)
                return;

            foreach (var id in FindRemovals(torrents))
            {
                statusMap.Remove(id);
            }

            foreach (var torrent in torrents)
            {
                // If the torrent isn't in the status map, add it and associate
                // its status.
                if (!statusMap.ContainsKey(torrent.Id))
                {
                    statusMap.Add(torrent.Id, torrent.Status);
                }
                // Otherwise, check the torrent's current status against what's
                // in the status map.
                else
                {
                    // Look at what the torrent had before, then compare it to
                    // what status the torrent has now.
                    switch (statusMap[torrent.Id])
                    {
                        case TorrentStatus.Downloading:
                            // Previously, this torrent was downloading. Are we
                            // still downloading? Don't depend on just the
                            // status change of Downloading, but also look at
                            // the other indicators that the server provides,
                            // just to be sure
                            if (torrent.Status != TorrentStatus.Downloading &&
                                (torrent.IsFinished || torrent.PercentDone >= 1.0))
                            {
                                notificationService.ReportDownloadComplete(torrent.Name);
                            }
                            break;
                        case TorrentStatus.Seeding:
                            // Seeding is a special case. Just stopping the
                            // torrent from seeding won't be reported unless the
                            // seed ratio has been reached. Recall that the seed
                            // ratio is dependent on a number of factors. See
                            // the property in the Torrent class for details.
                            if (torrent.Status != TorrentStatus.Seeding &&
                                torrent.SeedRatioProgress >= MinimumSeeding)
                            {
                                notificationService.ReportSeedingComplete(torrent.Name);
                            }
                            break;
                    }
                    statusMap[torrent.Id] = torrent.Status;
                }
            }
        }
    }
}
