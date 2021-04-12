using System;
using System.Linq;

namespace Tidal.Helpers
{
    public static class PropertyHelpers
    {
        public static string GetPropertyAttribute<TObj, TAttr>(string propertyName, Func<TAttr, string> selector)
        {
            var prop = typeof(TObj).GetProperty(propertyName);
            if (prop is null)
                return propertyName;

            var attrs = prop.GetCustomAttributes(typeof(TAttr), false);
            if (attrs != null && attrs.Count() > 0)
            {
                return selector((TAttr)attrs[0]);
            }
            return propertyName;
        }
    }
}
