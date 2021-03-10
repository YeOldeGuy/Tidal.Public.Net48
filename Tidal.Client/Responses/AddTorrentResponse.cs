using System.Runtime.Serialization;
using Tidal.Client.Constants;

namespace Tidal.Client.Responses
{
    public class CommonArgs
    {
        [DataMember(Name = RpcConstants.HashString)]
        public string HashString { get; set; }

        [DataMember(Name = RpcConstants.Id)]
        public int Id { get; set; }

        [DataMember(Name = RpcConstants.Name)]
        public string Name { get; set; }
    }

    public class TorrentAdded
    {
        [DataMember(Name = RpcConstants.TorrentAdded)]
        public CommonArgs Args { get; set; }
    }

    public class TorrentDuplicate
    {
        [DataMember(Name = RpcConstants.TorrentDuped)]
        public CommonArgs Args { get; set; }
    }

    public class AddTorrentAdded : ResponseBase
    {
        [DataMember(Name = RpcConstants.Arguments)]
        public TorrentAdded TorrentAdded { get; set; }
    }

    public class DupTorrentAdded : ResponseBase
    {
        [DataMember(Name = RpcConstants.Arguments)]
        public TorrentDuplicate TorrentDuplicate { get; set; }
    }

    /// <summary>
    /// Represents the response when a torrent is added. Used for both adding
    /// torrents and magnet links.
    /// </summary>
    /// <remarks>
    /// This has to be able to handle this typical response:
    /// <code>
    /// {
    ///     "arguments": {
    ///         "torrent-duplicate": {
    ///             "hashString": "2fa71a2dbb7d53a39373a9c4e2c9d89aaa7a6db1",
    ///             "id": 6,
    ///             "name": "checkMyTorrentIp.png"
    ///         }
    ///     },
    ///     "result": "success",
    ///     "tag": 3
    /// }
    /// </code>
    /// If the torrent *wasn't* a duplicate, then the <c>"torrent-duplicate"</c> tag
    /// will be labeled with <c>"torrent-added"</c>, but otherwise will be the same.
    /// </remarks>
    public class AddTorrentResponse : ResponseBase
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string HashString { get; set; }

        public bool IsDuplicate { get; set; }


        public override void InPlaceDeserialize(string json)
        {
            if (json.Contains(RpcConstants.TorrentAdded))
            {
                IsDuplicate = false;
                var args = Deserialize<AddTorrentAdded>(json);
                Id = args.TorrentAdded.Args.Id;
                Name = args.TorrentAdded.Args.Name;
                HashString = args.TorrentAdded.Args.HashString;
            }
            else if (json.Contains(RpcConstants.TorrentDuped))
            {
                IsDuplicate = true;
                var args = Deserialize<DupTorrentAdded>(json);
                Id = args.TorrentDuplicate.Args.Id;
                Name = args.TorrentDuplicate.Args.Name;
                HashString = args.TorrentDuplicate.Args.HashString;
            }
        }
    }
}
