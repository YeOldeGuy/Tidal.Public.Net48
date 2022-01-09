using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Core.Helpers;

namespace Tidal.Client.Models
{
    /// <summary>
    /// Represents one peer for a torrent.
    /// </summary>
    /// <remarks>
    /// See section 3.3 of rpc-spec.txt, down in the "peers" array for an
    /// overview of this class. Note that not all fields are represented.
    /// </remarks>
    public class Peer : Assignable<Peer>, IEquatable<Peer>
    {
        #region Backing Store
        private int? hashCode;
        private int _OwnerId;
        private string _OwnerName;
        private string _Address;
        private long _AverageToClient;
        private long _AverageToPeer;
        private string _ClientName;
        private string _Flags;
        private bool _IsEncrypted;
        private double _Progress;
        private int _Port;
        private string _Location;
        private bool _LocationValid;
        private long _RateToClient;
        private long _RateToPeer;
        private object _Geo;
        #endregion Backing Store

        // These MovingAverage values are only allocated on demand. The peers that
        // are being retrieved from the client won't need them. One other instance
        // will be the one that is being displayed somewhere; that one gets values
        // assigned to it via Assign(), and will get the averages created there.
        //

        private MovingAverage upAverage;
        private MovingAverage downAverage;

        #region IEquatable
        public bool Equals(Peer other) => !(other is null) && GetHashCode().Equals(other.GetHashCode());

        public override bool Equals(object obj) => !(obj is null) && obj is Peer peer && Equals(peer);

        public override int GetHashCode()
        {
            return hashCode ?? (hashCode = $"{OwnerId}:{Address}".GetHashCode()).Value;
        }
        #endregion IEquatable

        #region Overrides
        protected override void AssignInternal(Peer other)
        {
            Address = other.Address;
            ClientName = other.ClientName;
            Progress = other.Progress;
            Port = other.Port;
            RateToClient = other.RateToClient;
            RateToPeer = other.RateToPeer;
            IsEncrypted = other.IsEncrypted;
            Flags = other.Flags;

            if (upAverage == null) upAverage = new MovingAverage();
            if (downAverage == null) downAverage = new MovingAverage();

            AverageToClient = (long)downAverage.Push(other.RateToClient);
            AverageToPeer = (long)upAverage.Push(other.RateToPeer);
        }
        #endregion Overrides

        #region Synthetic Properties

        /// <summary>
        /// The id number of the torrent that this peer is associated with.
        /// </summary>
        [IgnoreDataMember]
        public int OwnerId
        {
            get => _OwnerId;
            set => SetProperty(ref _OwnerId, value);
        }

        /// <summary>
        /// The torrent name that this peer is associated with.
        /// </summary>
        [IgnoreDataMember]
        public string OwnerName
        {
            get => _OwnerName;
            set => SetProperty(ref _OwnerName, value);
        }

        /// <summary>
        /// A <see cref="MovingAverage"/> of the download rate.
        /// </summary>
        [IgnoreDataMember, Description("Average Download Rate")]
        public long AverageToClient
        {
            get => _AverageToClient == 0 && RateToClient > 0 ? RateToClient : _AverageToClient;
            set => SetProperty(ref _AverageToClient, value);
        }

        /// <summary>
        /// A <see cref="MovingAverage"/> of the upload rate.
        /// </summary>
        [IgnoreDataMember, Description("Average Upload Rate")]
        public long AverageToPeer
        {
            get => _AverageToPeer == 0 && RateToPeer > 0 ? RateToPeer : _AverageToPeer;
            set => SetProperty(ref _AverageToPeer, value);
        }

        [IgnoreDataMember, Description("Average Upload Rate")]
        public long AverageTopeer
        {
            // Somewhere out there, hidden safely away from prying eyes,
            // is a binding reference to this particular misspelling of
            // the AverageToPeer (note the lack of uppercase on "Peer")
            // Damn if I can find it.

            get => _AverageToPeer == 0 && RateToPeer > 0 ? RateToPeer : _AverageToPeer;
            set => SetProperty(ref _AverageToPeer, value);
        }

        /// <summary>
        /// The GeoIP2 location as found. This is an <see cref="object"/> so
        /// that the MaxMind library doesn't have to be referenced here.
        /// </summary>
        [IgnoreDataMember]
        public object Geo
        {
            get => _Geo;
            set => SetProperty(ref _Geo, value);
        }

        /// <summary>
        /// A string representing the peer's location.
        /// </summary>
        [IgnoreDataMember, Description("Location")]
        public string Location
        {
            get => _Location;
            set => SetProperty(ref _Location, value);
        }

        /// <summary>
        /// Is the location for the peer a valid one?
        /// </summary>
        [IgnoreDataMember]
        public bool LocationValid
        {
            get => _LocationValid;
            set => SetProperty(ref _LocationValid, value);
        }
        #endregion Synthetic Properties

        #region JSON Properties
        /// <summary>
        /// The IP address (dotted notation) of the peer.
        /// </summary>
        [DataMember(Name = RpcConstants.Address), Description("IP Address")]
        public string Address
        {
            get => _Address;
            set => SetProperty(ref _Address, value);
        }

        /// <summary>
        /// The client the peer is using. This may be garbage for certain peers
        /// who try to disguise the app being used.
        /// </summary>
        [DataMember(Name = RpcConstants.ClientName), Description("Client Name")]
        public string ClientName
        {
            get => _ClientName;
            set => SetProperty(ref _ClientName, value);
        }

        /// <summary>
        /// A string of single character flags.
        /// </summary>
        /// <remarks>
        /// The characters are as follows (from the uTorrent docs):
        /// <list type="table">
        ///   <listheader>
        ///     <term>Character</term>
        ///     <description>Meaning</description>
        ///   </listheader>
        ///   <item>
        ///     <term>D</term>
        ///     <description>Currently downloading (interested and not choked)</description>
        ///   </item>
        ///   <item>
        ///     <term>d</term>
        ///     <description>Your client wants to download, but peer doesn't want to send (interested and choked)</description>
        ///   </item>
        ///   <item>
        ///     <term>U</term>
        ///     <description>Currently uploading (interested and not choked)</description>
        ///   </item>
        ///   <item>
        ///     <term>u</term>
        ///     <description>Peer wants your client to upload, but your client doesn't want to (interested and choked)</description>
        ///   </item>
        ///   <item>
        ///     <term>O</term>
        ///     <description>Optimistic unchoke</description>
        ///   </item>
        ///   <item>
        ///     <term>S</term>
        ///     <description>Peer is snubbed</description>
        ///   </item>
        ///   <item>
        ///     <term>I</term>
        ///     <description>Peer is an incoming connection</description>
        ///   </item>
        ///   <item>
        ///     <term>K</term>
        ///     <description>Peer is unchoking your client, but your client is not interested</description>
        ///   </item>
        ///   <item>
        ///     <term>?</term>
        ///     <description>Your client unchoked the peer but the peer is not interested</description>
        ///   </item>
        ///   <item>
        ///     <term>X</term>
        ///     <description>Peer was included in peerlists obtained through Peer Exchange (PEX)</description>
        ///   </item>
        ///   <item>
        ///     <term>H</term>
        ///     <description>Peer was obtained through DHT</description>
        ///   </item>
        ///   <item>
        ///     <term>E</term>
        ///     <description>Peer is using Protocol Encryption (all traffic)</description>
        ///   </item>
        ///   <item>
        ///     <term>e</term>
        ///     <description>Peer is using Protocol Encryption (handshake)</description>
        ///   </item>
        ///   <item>
        ///     <term>P</term>
        ///     <description>Peer is using uTorrent uTP</description>
        ///   </item>
        ///   <item>
        ///     <term>L</term>
        ///     <description>Peer is local (discovered through network broadcast, or in reserved local IP ranges)</description>
        ///   </item>
        /// </list>
        /// </remarks>
        [DataMember(Name = RpcConstants.FlagString)]
        public string Flags
        {
            get => _Flags;
            set => SetProperty(ref _Flags, value);
        }

        /// <summary>
        /// Is the connection to the peer encrypted?
        /// </summary>
        [DataMember(Name = RpcConstants.IsEncrypted)]
        public bool IsEncrypted
        {
            get => _IsEncrypted;
            set => SetProperty(ref _IsEncrypted, value);
        }

        /// <summary>
        /// What port is the peer using?
        /// </summary>
        [DataMember(Name = RpcConstants.Port)]
        public int Port
        {
            get => _Port;
            set => SetProperty(ref _Port, value);
        }

        /// <summary>
        /// How far along is the peer's download progress?
        /// </summary>
        [DataMember(Name = RpcConstants.Progress)]
        public double Progress
        {
            get => _Progress;
            set => SetProperty(ref _Progress, value);
        }

        /// <summary>
        /// This is the download rate from the peer to the client
        /// </summary>
        [DataMember(Name = RpcConstants.RateToClient)]
        public long RateToClient
        {
            get => _RateToClient;
            set => SetProperty(ref _RateToClient, value);
        }

        /// <summary>
        /// This is the upload rate from the client to the peer.
        /// </summary>
        [DataMember(Name = RpcConstants.RateToPeer)]
        public long RateToPeer
        {
            get => _RateToPeer;
            set => SetProperty(ref _RateToPeer, value);
        }
        #endregion JSON Properties
    }
}
