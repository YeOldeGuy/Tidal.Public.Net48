using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Tidal.Models;

namespace Tidal.Services.Abstract
{
    // Please Note:
    // 
    // As of Jan 1, 2020, the GeoIP2 database files are no longer freely
    // available. They are still available for no cost, but now, you must give
    // them your name and address, for which in return, they'll give you a token
    // that allows you to download the file.
    //
    // <rant>
    //
    // This is being done in the interests of privacy, MaxMind say. Odd that in
    // this day of rampant system infiltrations and compromises, they want to
    // collect a bunch of personal data, but that's the backward world we live
    // in.
    //
    // War is peace. Freedom is slavery. Collecting personal data is privacy.
    //
    // </rant>
    //
    // So, the original method is still there, LoadDbFileAsync, in case you're
    // self-hosting the mmdb file, but there's also the new DownloadMaxMindAsync
    // method now, too. That one requires you to specify a username, password
    // and license key as issued by MaxMind when you gave them your personal
    // info for safe keeping.
    //

    /// <summary>
    /// Completion notification data of GeoIP2 DB file. Default constructor
    /// assumes a failed download/unzip.
    /// </summary>
    public class GeoDownloadedEvent : EventArgs
    {
        public static GeoDownloadedEvent FailedDownload => new GeoDownloadedEvent() { Date = DateTime.Now, Success = false, };

        public bool Success { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime Date { get; set; }
    }

    public interface IGeoService : INotifyPropertyChanged
    {
        /// <summary>
        /// If <see langword="true"/>, then the <see cref="IGeoService"/> is
        /// currently downloading or is busy emplacing the MMDB file.
        /// </summary>
        bool IsDownloading { get; }

        /// <summary>
        /// Force the system to download a new DB file.
        /// </summary>
        Task RefreshDBFile();

        /// <summary>
        /// Download the GeoIP2 file from MaxMind using the information
        /// specified. 
        /// </summary>
        /// <remarks>
        /// The parameters must have a valid username, password, and
        /// license key issued by MaxMind.
        /// </remarks>
        /// <returns><see langword="True"/> if successful.</returns>
        Task<bool> DownloadMaxMindAsync(string username, string password, string licenseKey);

        /// <summary>
        ///   Unzips and installs a GeoIP2 file from MaxMind. The file needs to
        ///   have the ".mmdb.gz" file suffix or an error will occur.
        /// </summary>
        /// <param name="compressedDbFile">
        ///   An accessible file path.
        /// </param>
        /// <param name="deleteOriginal">
        ///   If true, remove the original file after copying.
        /// </param>
        /// <returns>
        ///   Returns <see langword="true"/> if the file was installed
        ///   correctly.
        /// </returns>
        Task<bool> LoadDbFileAsync(string compressedDbFile, bool deleteOriginal = false);

        /// <summary>
        /// Gets the age of the GeoIP2 file.
        /// </summary>
        /// <remarks>
        /// This is <b>not</b> the time measured against the time it was
        /// downloaded, but rather the time since MaxMind created the database.
        /// You might download a DB file and have it be six days old already.
        /// </remarks>
        TimeSpan DBFileAge { get; }

        /// <summary>
        /// Convert the specified <see cref="GeoLocation"/> to a string
        /// representation.
        /// </summary>
        /// <param name="location">A <see cref="GeoLocation"/> instance.</param>
        /// <returns>
        ///   A string with the form of <c>"City, Country"</c> or <c>"City,
        ///   State, Country"</c> for locations in the US or Canada.
        /// </returns>
        string GetFullLocation(GeoLocation location);

        /// <summary>
        /// Asynchronously gets the <see cref="GeoLocation"/> information
        /// for the specified <paramref name="ipAddress"/>.
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns>A <see cref="GeoLocation"/> instance or <see langword="nil"/>.</returns>
        Task<GeoLocation> GetRawLocationAsync(string ipAddress);

        /// <summary>
        /// Gets the <see cref="GeoLocation"/> information for the specified
        /// <paramref name="ipAddress"/>.
        /// </summary>
        /// <remarks>
        /// If the GeoIP2 file is not found, this will return <see
        /// langword="null"/>.
        /// </remarks>
        /// <param name="ipAddress"></param>
        /// <returns>A fully-populated <see cref="GeoLocation"/>, or
        /// <see langword="null"/>.</returns>
        GeoLocation GetRawLocation(string ipAddress);

        /// <summary>
        /// Asynchronously gets the geographical location of
        /// the specified <paramref name="ipAddress"/>.
        /// </summary>
        /// <param name="ipAddress">An IPv4 address in dotted notation.</param>
        /// <returns>A string with the location.</returns>
        Task<string> GetFormattedLocationAsync(string ipAddress);

        string GetFormattedLocation(string ipAddress);
    }
}
