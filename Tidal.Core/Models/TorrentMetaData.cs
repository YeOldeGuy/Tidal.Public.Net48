using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Tidal.Core.Models
{
    /// <summary>
    /// Represents the data held in a .torrent file. This is meant
    /// to be populated by the json library.
    /// </summary>
    public class TorrentMetadata
    {
        /// <summary>
        /// Information pertaining to the files within the torrent.
        /// </summary>
        public class TorFileInfo
        {
            /// <summary>
            /// Represents the data for an individual file in a
            /// multiple-file format torrent.
            /// </summary>
            public class File
            {
                [DataMember(Name = "path")]
                public IList<string> Path { get; set; }

                [DataMember(Name = "length")]
                public long Length { get; set; }

                public string FullPath => Path == null ? null : string.Join("/", Path);
            }

            /// <summary>
            /// Single-file format: The name of the file.
            /// <para/>
            /// Multiple-file format: The name of the proposed directory for the files.
            /// </summary>
            [DataMember(Name = "name")]
            public string Name { get; set; }

            [DataMember(Name = "length")]
            public long LengthRaw { get; set; }


            /// <summary>
            /// Single-file format: The length of the file.
            /// Multiple-file format: The sum of lengths of the files.
            /// </summary>
            public long Length
            {
                get
                {
                    if (LengthRaw == 0)
                        LengthRaw = (from f in Files
                                     select f.Length).Sum();
                    return LengthRaw;
                }
                private set { LengthRaw = value; }
            }

            /// <summary>
            /// Length in bytes of the individual pieces of the file.
            /// </summary>
            [DataMember(Name = "piece length")]
            public int PieceLength { get; set; }

            /// <summary>
            /// A concatenation of all of the SHA-1 hashes of the pieces
            /// of the files of the finished torrent.
            /// </summary>
            [DataMember(Name = "pieces")]
            public string PiecesRaw { get; set; }


            private byte[][] piecesStaging;
            /// <summary>
            /// An array of SHA-1 hashes of the pieces of the finished torrent.
            /// Each hash is represented as an array of 20 bytes, no more, no
            /// less, except for the final hash, which may be shorter.
            /// </summary>
            public byte[][] Pieces
            {
                get
                {
                    if (piecesStaging == null)
                    {
                        char[] chars = PiecesRaw.ToCharArray();

                        var count = PiecesRaw.Length / 20;
                        piecesStaging = new byte[count][];
                        for (int i = 0; i < count; i++)
                        {
                            piecesStaging[i] = Encoding.ASCII.GetBytes(chars, i * 20, 20);
                        }
                    }
                    return piecesStaging;
                }
            }

            /// <summary>
            /// Number of pieces in the torrent.
            /// </summary>
            public int PieceCount
            {
                get => PiecesRaw.Length / 20;
            }

            [DataMember(Name = "sha1")]
            public string Sha1 { get; set; }

            [DataMember(Name = "sha256")]
            public string Sha256 { get; set; }

            [DataMember(Name = "files")]
            public IList<File> Files { get; set; }

            [DataMember(Name = "private")]
            public int PrivateRaw { get; set; }
            public bool Private => PrivateRaw != 0;
        }


        [DataMember(Name = "announce")]
        public string Announce { get; set; }


        [DataMember(Name = "info")]
        public TorFileInfo Info { get; set; }

        /// <summary>
        /// A list of lists of URLs, each consisting of a main announce,
        /// followed by more backup announces (usually empty).
        /// </summary>
        /// <remarks>
        /// As per http://www.bittorrent.org/beps/bep_0012.html
        /// </remarks>
        [DataMember(Name = "announce-list")]
        public IList<IList<string>> AnnounceList { get; set; }

        [DataMember(Name = "httpseeds")]
        public IList<string> HttpSeeds { get; set; }

        [DataMember(Name = "comment")]
        public string Comment { get; set; }


        [DataMember(Name = "creation date")]
        public long CreationDateRaw { get; set; }
        public DateTime CreationDate
        {
            get { return DateTimeOffset.FromUnixTimeSeconds(CreationDateRaw).ToLocalTime().DateTime; }
        }
    }
}
