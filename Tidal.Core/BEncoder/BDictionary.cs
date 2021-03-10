using System;
using System.Collections.Generic;
using System.Text;

namespace Tidal.Core.BEncoder
{
    /// <summary>
    /// Represents a dictionary of values. The keys are <see cref="BString"/>
    /// instances, the values are any <see cref="IBElement"/>, including another
    /// <see cref="BDictionary"/>.
    /// </summary>
    public class BDictionary : Dictionary<BString, IBElement>, IBElement
    {
        /// <summary>
        /// Add a keyed <see cref="IBElement"/> to the dictionary.
        /// </summary>
        /// <param name="key">A dictionary key string.</param>
        /// <param name="element">Any <see cref="IBElement"/>.</param>
        public void Add(string key, IBElement element) => Add(new BString(key), element);

        /// <summary>
        /// Write our data to the <see cref="StringBuilder"/> specified.
        /// Dictionaries in Json are delimited with curly brackets.
        /// </summary>
        /// <param name="sb"></param>
        public void BuildJson(StringBuilder sb)
        {
            _ = sb ?? throw new ArgumentNullException(nameof(sb));

            sb.Append('{');
            foreach (var key in Keys)
            {
                key.BuildJson(sb);
                sb.Append(':');
                this[key].BuildJson(sb);
                sb.Append(',');
            }
            if (Keys.Count > 0)
                sb.Remove(sb.Length - 1, 1); // delete final comma
            sb.Append('}');
        }
    }
}
