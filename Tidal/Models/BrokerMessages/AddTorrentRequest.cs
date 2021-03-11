using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tidal.Client;
using Tidal.Services.Abstract;
using Tidal.Helpers;

namespace Tidal.Models.BrokerMessages
{
    internal class AddTorrentRequest : BrokerRequestBase
    {
        /// <summary>
        /// Create a new request for adding torrents using the specified file.
        /// </summary>
        /// <param name="filepath">A fully-qualified file path name.</param>
        /// <param name="unwanted">Index numbers of files in the request that should not be downloaded.</param>
        /// <param name="paused">If <see langword="true"/> then add, but don't start the torrent.</param>
        public AddTorrentRequest(string filepath, IEnumerable<int> unwanted, bool paused = false)
        {
            IFileService fileService = ServiceResolver.Resolve<IFileService>();
            if (fileService == null)
                throw new InvalidOperationException("No IFileService found from AddTorrentRequest");

            Paused = paused;
            FilesUnwanted = unwanted.ToList();
            byte[] bytes = fileService.ReadAllBytes(filepath, StorageStrategy.None);

            if (bytes != null && bytes.Length > 0)
                Base64Rep = Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// If the constructor was successful, then this is the Base64
        /// representation of the contents of the file, necessary for the client
        /// RPCs.
        /// </summary>
        public string Base64Rep { get; }

        /// <summary>
        /// Set this to <see langword="true"/> if the torrent is to be added but
        /// not started.
        /// </summary>
        public bool Paused { get; }

        /// <summary>
        /// A list of the file indexes that should not be started while the rest
        /// of the files in the torrent are.
        /// </summary>
        public IList<int> FilesUnwanted { get; }

        public override async Task Invoke(IClient client)
        {
            var localClient = client;
            await InvokeWrapper(async () =>
            {
                (var tor, var isDup) = await localClient.AddTorrentAsync(Base64Rep, Paused, FilesUnwanted);
                Messenger.Send(new AddTorrentResponse(tor, isDup));
            });
        }
    }
}
