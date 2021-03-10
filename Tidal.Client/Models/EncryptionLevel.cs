using System.ComponentModel;

namespace Tidal.Client.Models
{
    /// <summary>
    /// The three levels of encryption the host can mandate.
    /// </summary>
    public enum EncryptionLevel
    {
        /// <summary>
        /// Host will use encryption, peers must use encryption.
        /// </summary>
        [Description(Constants.RpcConstants.EncryptionRequired)]
        Required,

        /// <summary>
        /// Host will use encryption, peers may use encryption.
        /// </summary>
        [Description(Constants.RpcConstants.EncryptionPreferred)]
        Preferred,

        /// <summary>
        /// Host will not use encryption, peers may use encryption.
        /// </summary>
        [Description(Constants.RpcConstants.EncryptionTolerated)]
        Tolerated,
    }
}
