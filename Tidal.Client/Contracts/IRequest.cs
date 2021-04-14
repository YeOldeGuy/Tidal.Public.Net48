namespace Tidal.Client.Contracts
{
    /// <summary>
    /// Represents the request data that all requests made of the
    /// transmission client must have.
    /// </summary>
    public interface IRequest
    {
        /// <summary>
        /// One of the RPC commands defined in rpc-spec.txt, like
        /// "torrent-get".
        /// </summary>
        string Method
        {
            get;
        }

        /// <summary>
        /// Set by the <see cref="IClient"/> instance, identifies a
        /// request with the corresponding response having the same value.
        /// </summary>
        long? Tag
        {
            get; set;
        }

        /// <summary>
        /// Turn the request into a JSON-encoded string.
        /// </summary>
        /// <returns>A JSON-encoded representation of the request.</returns>
        string Serialize();
    }
}
