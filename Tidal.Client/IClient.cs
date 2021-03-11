using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tidal.Client.Models;

namespace Tidal.Client
{
    public interface IClient : IDisposable
    {
        /// <summary>
        /// Gets the <see cref="Uri"/> of the host as specified by
        /// <see cref="SetHost(string, int, bool)"/>.
        /// </summary>
        Uri Host { get; }

        /// <summary>
        /// Gets only the IP address of the <see cref="Host"/>.
        /// </summary>
        string IpAddress { get; }


        /// <summary>
        /// If <see langword="true"/>, the last communication with the
        /// host was successful.
        /// </summary>
        bool IsOpen { get; }


        /// <summary>
        /// Gets the port number used to connect to the host.
        /// </summary>
        int Port { get; }

        /// <summary>
        /// If <see langword="true"/>, the connection to the host is
        /// using secure protocols.
        /// </summary>
        bool SecureConnection { get; }


        /// <summary>
        /// Gets or sets the time that can elapse without a response
        /// from the host.
        /// </summary>
        TimeSpan Timeout { get; set; }


        /// <summary>
        /// Sets the host parameters.
        /// </summary>
        /// <param name="address">The IP address of the host</param>
        /// <param name="port">The port number, normally 9091</param>
        /// <param name="secure">If <see langword="true"/>, use https</param>
        void SetHost(string address, int port, bool secure = false);


        /// <summary>
        /// Set the user name and password of the connection, if used. 
        /// </summary>
        /// <param name="username">The user name to be used for authentication</param>
        /// <param name="password">The password to be used for authentication</param>
        void SetAuthorizationInfo(string username, string password);


        /// <summary>
        /// Clears all authentication of the connection, useful when switching
        /// from a authenticated host to one not.
        /// </summary>
        void ClearAuthorizationInfo();


        /// <summary>
        /// Attempts to connect to the host specified in <see
        /// cref="SetHost(string, int, bool)"/>.
        /// </summary>
        /// <returns><see langword="true"/> if a response from the host is received.</returns>
        Task<bool> TryOpenAsync();


        /// <summary>
        /// Attempts to add the specified magnet link to the host. If successful,
        /// the torrent will be started unless <paramref name="paused"/> is
        /// <see langword="true"/>.
        /// </summary>
        /// <param name="magnetLink">
        ///   A magnet link in URL format.
        /// </param>
        /// <param name="paused">
        ///   If <see langword="true"/>, add the magnet, but don't start it.
        /// </param>
        /// <returns>
        ///   A tuple consisting of an abbreviated <see cref="Torrent"/> with the
        ///   <see cref="Torrent.HashString"/>, <see cref="Torrent.Id"/>, and the
        ///   <see cref="Torrent.Name"/>. The second element of the tuple is a flag
        ///   that is set to <see langword="true"/> if the magnet link was already
        ///   added to the host.
        /// </returns>
        Task<(Torrent added, bool isDuplicate)> AddMagnetAsync(string magnetLink, bool paused);


        /// <summary>
        ///   Attempts to add the specified torrent to the host. If successful,
        ///   the torrent will be started unless <paramref name="paused"/> is
        /// <see langword="true"/>.
        /// </summary>
        /// <param name="base64">
        ///   The contents of the torrent file encoded as a base-64 string.
        /// </param>
        /// <param name="paused">
        ///   If <see langword="true"/>, add the magnet, but don't start it.
        /// </param>
        /// <param name="unwantedIndexes">
        ///   If the torrent consists of more than one file, any offset indexes
        ///   listed here will not be downloaded when started. This can be
        ///   changed later with <see cref="SetTorrentAsync(TorrentMutator)"/>.
        /// </param>
        /// <returns>
        ///   A tuple consisting of an abbreviated <see cref="Torrent"/> with
        ///   the <see cref="Torrent.HashString"/>, <see cref="Torrent.Id"/>,
        ///   and the <see cref="Torrent.Name"/>. The second element of the
        ///   tuple is a flag that is set to <see langword="true"/> if the
        ///   magnet link was already added to the host.
        /// </returns>
        Task<(Torrent added, bool isDuplicate)> AddTorrentAsync(string base64, bool paused, IEnumerable<int> unwantedIndexes);

        /// <summary>
        /// Gets the current <see cref="Session"/> value from the host.
        /// </summary>
        /// <returns></returns>
        Task<Session> GetSessionAsync();

        /// <summary>
        /// Gets the <see cref="SessionStats"/> from the host.
        /// </summary>
        /// <returns></returns>
        Task<SessionStats> GetStatsAsync();


        /// <summary>
        /// Gets the specified set of <see cref="Torrent"/>s. Defaults to retrieving all.
        /// </summary>
        /// <param name="ids">A collection of <see cref="Torrent.Id"/> values to retrieve.</param>
        /// <param name="fields">A collection of JSON identifiers as in rpc-spec.txt</param>
        /// <returns>A collection of <see cref="Torrent"/> instances.</returns>
        Task<IEnumerable<Torrent>> GetTorrentsAsync(IEnumerable<int> ids = null, IEnumerable<string> fields = null);


        /// <summary>
        /// Ask the host to reannounce the specified <see cref="Torrent"/>s.
        /// </summary>
        /// <remarks>
        ///   Reannouncing is the equivalent of asking for more peers.
        /// </remarks>
        /// <param name="ids">
        ///   A collection of <see cref="Torrent.Id"/> values to announce.
        /// </param>
        Task ReannounceTorrentsAsync(IEnumerable<int> ids);


        /// <summary>
        /// Remove the specified <see cref="Torrent"/>s from the host.
        /// Optionally, delete the data, too.
        /// </summary>
        /// <param name="ids">
        ///   A collection of <see cref="Torrent.Id"/> values to remove.
        /// </param>
        /// <param name="deleteData">
        ///   If <see langword="true"/>, delete the associated data files
        ///   for the torrent.
        /// </param>
        Task RemoveTorrentsAsync(IEnumerable<int> ids, bool deleteData);


        /// <summary>
        ///   Change the current <see cref="Session"/> settings on the host.
        /// </summary>
        /// <param name="mutator">
        ///   A partially filled <see cref="SessionMutator"/>. Leave fields that
        ///   are not to be changed as <see langword="null"/>.
        /// </param>
        Task SetSessionAsync(SessionMutator mutator);


        /// <summary>
        ///   Change values of the specified <see cref="Torrent"/>s via a <see
        ///   cref="TorrentMutator"/>. Set only the values to be changed,
        ///   leaving others as <see langword="null"/>.
        /// </summary>
        /// <remarks>
        ///   The <paramref name="mutator"/> has a field, <see
        ///   cref="TorrentMutator.Ids"/>, which has a collection of the <see
        ///   cref="Torrent.Id"/> values to change.
        /// </remarks>
        /// <param name="mutator">
        ///   A <see cref="TorrentMutator"/> with the requested values to
        ///   change. The <see cref="TorrentMutator.Ids"/> collection must be
        ///   set.
        /// </param>
        Task SetTorrentAsync(TorrentMutator mutator);


        /// <summary>
        ///   Request the host to start the specified <see cref="Torrent"/>s.
        /// </summary>
        /// <remarks>
        ///   The host may not start some or all of the torrents, depending on
        ///   the number of items in the download queue.
        /// </remarks>
        /// <param name="ids">
        ///   A collection of <see cref="Torrent.Id"/> value to request
        ///   starting.
        /// </param>
        Task StartTorrentsAsync(IEnumerable<int> ids);


        /// <summary>
        ///   Request the host to stop the specified <see cref="Torrent"/>s.
        /// </summary>
        /// <remarks>
        ///   It is not an error to stop an already stopped torrent.
        /// </remarks>
        /// <param name="ids">
        ///   A collection of <see cref="Torrent.Id"/> value to stop.
        /// </param>
        Task StopTorrentsAsync(IEnumerable<int> ids);

        Task<long> GetFreeSpaceAsync(string downloadDirectory);
    }
}
