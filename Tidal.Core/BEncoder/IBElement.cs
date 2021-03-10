using System.Text;

namespace Tidal.Core.BEncoder
{
    /// <summary>
    /// Base definition for all BEncoded elements
    /// </summary>
    public interface IBElement
    {
        /// <summary>
        ///   Write our data to the <see cref="StringBuilder"/> in a
        ///   Json-compatible fashion.
        /// </summary>
        /// <param name="sb">
        ///   The <see cref="StringBuilder"/> to append to.
        /// </param>
        void BuildJson(StringBuilder sb);
    }
}
