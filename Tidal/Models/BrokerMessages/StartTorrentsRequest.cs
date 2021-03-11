using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tidal.Client;
using Tidal.Client.Models;

namespace Tidal.Models.BrokerMessages
{
    /// <summary>
    /// A request message for starting specified torrents. It is not an error to
    /// specify torrents that are already started.
    /// </summary>
    internal class StartTorrentsRequest : BrokerRequestBase
    {
        /// <summary>
        ///   Create a request to have the host start the specified
        ///   <see cref="Torrent"/>s.
        /// </summary>
        /// <param name="torrents">
        ///   A collection of <see cref="Torrent"/> instances.
        /// </param>
        public StartTorrentsRequest(IEnumerable<Torrent> torrents)
        {
            Ids = torrents.Select(t => t.Id);
        }

        public IEnumerable<int> Ids { get; }

        public override async Task Invoke(IClient client)
        {
            var localClient = client;
            await InvokeWrapper(async () =>
            {
                await localClient.StartTorrentsAsync(Ids);
                // There is no response from the host when starting torrents.
                // The UI simply reflects that they're running now.
            });
        }
    }
}
