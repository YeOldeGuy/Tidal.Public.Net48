using System;
using System.Collections.Generic;
using System.Text;

namespace Tidal.Core.BEncoder
{
    /// <summary>
    /// Represents a list of <see cref="IBElement"/> values.
    /// </summary>
    class BList : List<IBElement>, IBElement
    {
        /// <summary>
        /// Writes the list to the <see cref="StringBuilder"/>, surrounded
        /// by square brackets.
        /// </summary>
        /// <param name="sb"></param>
        public void BuildJson(StringBuilder sb)
        {
            _ = sb ?? throw new ArgumentNullException(nameof(sb));

            sb.Append('[');
            foreach (var element in this)
            {
                element.BuildJson(sb);
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(']');
        }
    }
}
