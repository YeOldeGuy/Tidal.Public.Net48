using System;
using System.Threading.Tasks;
using Tidal.Client;

namespace Tidal.Models.BrokerMessages
{
    internal class AddMagnetRequest : BrokerRequestBase
    {
        /// <summary>
        ///   Create  a <see cref="AddMagnetRequest"/> consisting of the magnet
        ///   link plus a flag to tell the transmission client whether to start
        ///   the download or add it without starting.
        /// </summary>
        /// <param name="uri">
        ///   A specially formed <see cref="Uri"/> containing a magnet link.
        /// </param>
        /// <param name="paused">
        ///   If <see langword="true"/>, then the torrent is added to the
        ///   client, but not started.
        /// </param>
        public AddMagnetRequest(string uri, bool paused)
        {
            MagnetUri = uri;
            Paused = paused;
        }

        /// <summary>
        /// Gets the magnet link Uri.
        /// </summary>
        public string MagnetUri { get; }

        /// <summary>
        /// Gets the paused startup status of the magnet torrent.
        /// </summary>
        public bool Paused { get; }


        public override async Task Invoke(IClient client)
        {
            var localClient = client;
            await InvokeWrapper(async () =>
            {
                (var tor, var isDup) = await localClient.AddMagnetAsync(MagnetUri, Paused);
                Messenger.Send(new AddTorrentResponse(tor, isDup));
            });
        }
    }
}
