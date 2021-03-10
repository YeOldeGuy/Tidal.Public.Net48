using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tidal.Helpers;
using Tidal.Models;

namespace Tidal.Services.Abstract
{
    public enum ThemeMode
    {
        Light,
        Dark,
        SystemDefault,
    }


    public interface ISettingsService
    {
        /// <summary>
        /// Saves the settings to a file.
        /// </summary>
        void Save();

        /// <summary>
        /// Saves the settings to a file.
        /// </summary>
        Task SaveAsync();

        int Width { get; set; }
        int Height { get; set; }
        int Top { get; set; }
        int Left { get; set; }

        ThemeMode ThemeMode { get; set; }

        Uri SettingsPage { get; set; }

        Guid ActiveHost { get; set; }
        List<Host> Hosts { get; set; }

        List<string> SelectedHashes { get; set; }

        /// <summary>
        /// The name of the MaxMind GeoIP2 database file, sans path info.
        /// </summary>
        string GeoDbFileName { get; set; }

        /// <summary>
        /// The amount of time between checks to MaxMind for updates to
        /// the GeoIP2 database. Normally 7 days; they update it on each
        /// Tuesday.
        /// </summary>
        TimeSpan GeoIpRetrievalInterval { get; set; }

        /// <summary>
        /// The license key provided by MaxMind when you've signed up for
        /// getting the GeoLite2 databases. This should be an 18 character
        /// password-like string.
        /// </summary>
        string MaxMindLicenseKey { get; set; }

        /// <summary>
        /// The URL used to download the GeoLite2 database. This string should
        /// have a substring, "YOUR_LICENSE_KEY" that needs to have <see
        /// cref="MaxMindLicenseKey"/> substituted.
        /// </summary>
        string MaxMindPermaLink { get; set; }

        /// <summary>
        /// The login user name used to fetch the MaxMind database. This is the
        /// one used when you signed up for being able to download the file.
        /// </summary>
        string MaxMindUserName { get; set; }

        /// <summary>
        /// The login password used to fetch the MaxMind database. This is the
        /// one used when you signed up for being able to download the file.
        /// </summary>
        string MaxMindPassword { get; set; }

        TimeSpan DeadHostTime { get; set; }
        bool AggressiveGC { get; set; }
        TimeSpan PollingInterval { get; set; }

        string UploadPresets { get; set; }

        string DownloadPresets { get; set; }


        /// <summary>
        /// Contains information necessary to restore the layout of the
        /// XAML grids of the main page.
        /// </summary>
        LayoutInfo MainPageLayout { get; set; }

        /// <summary>
        /// Contains information necessary to restore the column widths and
        /// sort order of the torrent grid. This is a JSON string.
        /// </summary>
        string TorrentGridInfo { get; set; }

        /// <summary>
        /// Contains information necessary to restore the column widths and
        /// sort order of the per grid.
        /// </summary>
        string PeerGridInfo { get; set; }

        /// <summary>
        /// Contains information necessary to restore the column widths and
        /// sort order of the file grid.
        /// </summary>
        string FileGridInfo { get; set; }

    }
}
