using System;
using System.Collections.Generic;
using System.Web;

namespace Tidal.Core.Helpers
{
    public static class MagnetUtils
    {
        /// <summary>
        ///   Parse a magnet link into a series of dictionary entries.
        /// </summary>
        /// <remarks>
        ///   See https://en.wikipedia.org/wiki/Magnet_URI_scheme for details on
        ///   the various keys into the dictionary.
        /// </remarks>
        /// <param name="mag">A string containing the magnet link.</param>
        /// <returns>
        ///   A dictionary, which will be empty if the link is flawed, or <see
        ///   langword="null"/> if the <paramref name="mag"/> is empty or <see
        ///   langword="null"/>.
        /// </returns>
        public static Dictionary<string, string> ParseMagnetLink(string mag)
        {
            if (string.IsNullOrEmpty(mag))
                return null;

            var keyvaluepairs = HttpUtility.ParseQueryString(mag);
            var dict = new Dictionary<string, string>();
            for (int i = 0; i < keyvaluepairs.Count; i++)
            {
                var key = keyvaluepairs.Keys[i];
                var val = keyvaluepairs[i];

                if (key != null && val != null && !dict.ContainsKey(key))
                    dict.Add(key, val);
            }
            return dict;
        }

        /// <summary>
        ///    A quick check for magnet link validity. Does nothing more than look
        ///    for <c>magnet:?</c> as the first characters.
        /// </summary>
        /// <param name="magnet">The magnet link to check.</param>
        /// <returns>
        ///   <see langword="true"/> if the magnet link appears 
        ///   to be valid.
        /// </returns>
        public static bool IsValidMagnetUrl(string magnet)
        {
            return !(string.IsNullOrEmpty(magnet) ||
                   !magnet.StartsWith("magnet:?", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///   A more thorough test for magnet URL validity. Tests to see if the
        ///   initial protocol is correct, then looks for the "xt" or "dn"
        ///   field. This is in no fashion exhaustive; that is done by the host.
        /// </summary>
        /// <param name="magnet">A suspect magnet link.</param>
        /// <returns>
        ///   Return <see langword="true"/> if the link passes the tests, <see
        ///   langword="false"/> otherwise.
        /// </returns>
        public static bool IsValidMagnetUrl(string magnet,
                                            out Dictionary<string, string> dict)
        {
            dict = null;

            // Do some quick checks; don't parse the link unless necessary
            if (!IsValidMagnetUrl(magnet))
                return false;

            dict = ParseMagnetLink(magnet);
            return dict.Count != 0 && (dict.ContainsKey("xt") || dict.ContainsKey("dn"));
        }
    }
}
