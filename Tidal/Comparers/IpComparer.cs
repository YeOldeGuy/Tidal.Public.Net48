using System.ComponentModel;
using System.Linq;
using Tidal.Client.Models;
using Tidal.Collections;

namespace Tidal.Comparers
{
    /// <summary>
    /// Provides a custom sorter that understands that dotted-notation IP
    /// addresses should be sorted numerically and not alphanumerically.
    /// </summary>
    internal class IpComparer : ICustomSorter
    {
        /// <summary>
        /// Create a custom sorter that understands that dotted-notation IP
        /// addresses should be sorted numerically and not alphanumerically.
        /// </summary>
        public IpComparer() { }

        public ListSortDirection SortDirection { get; set; }

        public int Compare(object obj1, object obj2)
        {
            if (obj1 == null) return obj2 == null ? 0 : -1;
            if (obj2 == null) return 1;

            string a = "";
            string b = "";

            if (obj1 is string o1 && obj2 is string o2)
            {
                a = o1;
                b = o2;
            }

            if (obj1 is Peer peer1 && obj2 is Peer peer2)
            {
                a = peer1.Address;
                b = peer2.Address;
            }

            /*
             * Clever bit of code: http://stackoverflow.com/a/4785269
             *
             * Pulls apart each dotted value of the two IP addresses, then
             * for each pair, compare the integer values of them, exiting
             * at the first non-zero comparison. Returns 0 if the two are
             * equivalent, naturally (FirstOrDefault -> default(int) -> 0).
             */
            var res = Enumerable.Zip(a.Split('.'), b.Split('.'),
                                     (x, y) => int.Parse(x).CompareTo(int.Parse(y))).FirstOrDefault(i => i != 0);

            return res * (SortDirection == ListSortDirection.Ascending ? 1 : -1);
        }
    }
}
