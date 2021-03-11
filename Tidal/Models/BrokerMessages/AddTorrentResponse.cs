using Tidal.Client.Models;

namespace Tidal.Models.BrokerMessages
{
    internal class AddTorrentResponse
    {
        /// <summary>
        ///   Creates a response for when a torrent is added to the system. 
        /// </summary>
        /// <param name="id">
        ///   The torrent ID assigned by the client.
        /// </param>
        /// <param name="name">
        ///   The name of the torrent after processing by the client.
        /// </param>
        /// <param name="hash">
        ///   The <see cref="Torrent.HashString"/> of the torrent.
        /// </param>
        /// <param name="isDuplicate">
        ///   If set, the torrent was already added.
        /// </param>
        public AddTorrentResponse(int id, string name, string hash, bool isDuplicate = false)
        {
            Id = id;
            Name = name;
            HashString = hash;
            IsDuplicate = isDuplicate;
        }

        /// <summary>
        ///   An easier way to build an <see cref="AddTorrentRequest"/>.
        /// </summary>
        /// <param name="addedTorrent">
        ///   The torrent to derive necessary info from.
        /// </param>
        /// <param name="isDuplicate">
        ///   If set, the torrent is already in the system.
        /// </param>
        public AddTorrentResponse(Torrent addedTorrent, bool isDuplicate)
            : this(addedTorrent.Id, addedTorrent.Name, addedTorrent.HashString, isDuplicate)
        {
        }

        public bool IsDuplicate { get; }
        public int Id { get; }
        public string Name { get; }
        public string HashString { get; }
    }
}
