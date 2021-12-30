using System;
using System.ComponentModel;

namespace Tidal.Helpers
{
    /// <summary>
    /// Searches a specified class for a named property and if it
    /// has a specific <see cref="Attribute"/>, returns that value
    /// via a selector clause.
    /// </summary>
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
        ///   proper attribute or the property is not found.
        /// </returns>
        public static string GetPropertyAttribute<TObj, TAttr>(string propertyName, Func<TAttr, string> selector)
            where TObj : class
            where TAttr : Attribute
        {
            var prop = typeof(TObj).GetProperty(propertyName);
            if (prop is null)
                return propertyName;

            var attrs = prop.GetCustomAttributes(typeof(TAttr), false);
            return attrs?.Length > 0 ? selector((TAttr)attrs[0]) : propertyName;
        }

        /// <summary>
        /// Same thing as <see cref="GetPropertyAttribute{TObj, TAttr}(string, Func{TAttr, string})"/>
        /// with a selector of <see cref="DescriptionAttribute"/>.
        /// </summary>
        /// <typeparam name="TObj"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static string GetDescription<TObj>(string propertyName)
            where TObj : class
        {
            return GetPropertyAttribute<TObj, DescriptionAttribute>(propertyName, a => a.Description);
        }
    }
}
