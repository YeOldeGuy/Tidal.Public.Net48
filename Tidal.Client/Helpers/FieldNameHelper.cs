using System;
using System.Collections.Generic;
using System.Linq;

namespace Tidal.Client.Helpers
{
    public static class FieldNameHelper
    {
        /*
         * Currently, this app doesn't support an easy way to specify which of
         * the torrent fields to retrieve. Instead, by default, the app gets all
         * the fields of all the torrents. It does limit the fields retrieved to
         * only those fields defined in the Torrent class itself.
         *
         * While the Id's portion of the request can be left blank, signifying a
         * request for all torrents, the fields must be specified.
         *
         * GetFieldNames() looks at the properties of the type T for an
         * attribute of type U, then uses the selector to get the property name
         * from it.
         * 
         * Usage: 
         * var fields = GetFieldNames<Torrent, DataMemberAttribute>(a => a.Name);
         * 
         * This will return an IEnumerable<string> with all properties marked
         * with [DataMember(Name = something)].
         */
        public static IEnumerable<string> GetFieldNames<TObj, TAttr>(Func<TAttr, string> selector)
        {
            // Get all of the properties defined in the class T that are marked
            // with the JsonProperty attribute.
            var props = typeof(TObj).GetProperties()
                                    .Where(prop => Attribute.IsDefined(prop, typeof(TAttr)));

            // For those properties, get each of their property values,
            // gathering them into a list to return to the caller. (Do we still
            // call them "callers"? Probably not. Probably "invokers" or some
            // silly thing like that. Like "storage" being called "persist".)
            foreach (var p in props)
            {
                var attrs = p.GetCustomAttributes(typeof(TAttr), false).Cast<TAttr>();
                if (attrs.Count() > 0)
                    yield return selector(attrs.First());
            }
        }
    }
}
