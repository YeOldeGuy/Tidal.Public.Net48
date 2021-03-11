using System.ComponentModel;
using Tidal.Client.Models;
using Tidal.Collections;
using Tidal.Helpers;
using Tidal.Models;
using Tidal.Services.Abstract;

namespace Tidal.Comparers
{
    /// <summary>
    /// Provides a special version of <see cref="IComparer"/> that knows about
    /// the location data provided by the GeoIP2 library.
    /// </summary>
    internal class LocationComparer : ICustomSorter
    {
        /// <summary>
        /// Create a special comparer that sorts a location by country code,
        /// then by city. Special handling is provided for the US and Canada to
        /// also sort by state or province.
        /// </summary>
        public LocationComparer() { }

        private static IGeoService locationService;

        public ListSortDirection SortDirection { get; set; }

        public int Compare(object x, object y)
        {
            if (locationService == null)
            {
                locationService = ServiceResolver.Resolve<IGeoService>();
                if (locationService == null)
                    return 0;
            }

            if (x == null) return y == null ? 0 : -1;
            if (y == null) return 1;

            GeoLocation resp1 = null;
            GeoLocation resp2 = null;

            // Attempt to get the geo information for the two peers
            if (x is Peer peer1 && y is Peer peer2)
            {
                resp1 = peer1.Geo is GeoLocation geo1 ? geo1 : locationService.GetRawLocation(peer1.Address);
                resp2 = peer2.Geo is GeoLocation geo2 ? geo2 : locationService.GetRawLocation(peer2.Address);
            }
            else if (x is string s1 && y is string s2)
            {
                resp1 = GeoLocation.FromString(s1);
                resp2 = GeoLocation.FromString(s2);
            }

            // There's a chance that the geo info isn't available yet if the
            // mmdb file hasn't been downloaded yet. In that case, one or both
            // of the responses will be null. 
            if (resp1 == null) return resp2 == null ? 0 : -1;
            if (resp2 == null) return 1;

            // If we're here, the lookup worked. Start doing the comparisons. If
            // one or the other countries aren't valid, then the IP addresses
            // couldn't be found, most likely. Check for that here.
            if (!resp1.IsCountryValid)
                return !resp2.IsCountryValid ? 0 : -1;
            if (!resp2.IsCountryValid)
                return 1;

            // compare country names
            var res = resp1.Country.CompareTo(resp2.Country);
            // country names unequal? Use that for the sort.
            if (res != 0)
                return res * (SortDirection == ListSortDirection.Ascending ? 1 : -1);

            // Country names are equal. Are we in the US or Canada? Then compare
            // the state/province first and if they're unequal, use that.
            if (resp1.CountryCode == "US" || resp1.CountryCode == "CA")
            {
                if (resp1.IsSubDivisionValid && resp2.IsSubDivisionValid)
                {
                    res = resp1.SubDivision.CompareTo(resp2.SubDivision);
                    if (res != 0)
                        return res * (SortDirection == ListSortDirection.Ascending ? 1 : -1);
                }
            }

            // Maybe one or both of the cities isn't available
            if (!resp1.IsCityValid)
                return !resp2.IsCityValid ? 0 : -1;
            if (!resp2.IsCityValid)
                return 1;

            // Country names are equal, state codes are equal, so compare the
            // cities.
            return resp1.City.CompareTo(resp2.City) * (SortDirection == ListSortDirection.Ascending ? 1 : -1);
        }
    }
}
