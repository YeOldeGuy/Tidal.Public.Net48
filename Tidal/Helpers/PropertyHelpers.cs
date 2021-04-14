using System;
using System.ComponentModel;
using System.Linq;

namespace Tidal.Helpers
{
    public static class PropertyHelpers
    {
        /// <summary>
        ///   From the specified class, search for the named property, and if it
        ///   has the given <see cref="Attribute"/>, pass that to the <paramref
        ///   name="selector"/> function and return that value.
        /// </summary>
        /// <remarks>
        ///   Usage: <c>GetPropertyAttribute&lt;Torrent, DescriptionAttribute&gt;("ActivityDate", a => a.Description)</c>
        /// </remarks>
        /// <typeparam name="TObj">
        ///   Any class type with public properties defined.
        /// </typeparam>
        /// <typeparam name="TAttr">
        ///   An attribute attached to a property.
        /// </typeparam>
        /// <param name="propertyName">
        ///   The name of a property to search for.
        /// </param>
        /// <param name="selector">
        ///   A selection function.
        /// </param>
        /// <returns>
        ///   The value from the <paramref name="selector"/>. Returns the
        ///   property name if the specified property doesn't have the
        ///   proper attribute.
        /// </returns>
        public static string GetPropertyAttribute<TObj, TAttr>(string propertyName, Func<TAttr, string> selector)
            where TObj : class
            where TAttr : Attribute
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

        public static string GetDescription<TObj>(string propertyName)
            where TObj : class
        {
            return GetPropertyAttribute<TObj, DescriptionAttribute>(propertyName, a => a.Description);
        }
    }
}
