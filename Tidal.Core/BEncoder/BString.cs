using System;
using System.Text;
using System.Web;

namespace Tidal.Core.BEncoder
{
    /// <summary>
    /// Represents a string value. String values are escaped in
    /// a Json-compatible fashion.
    /// </summary>
    internal class BString : IBElement, IComparable<BString>
    {
        public BString(string value) { Value = value; }

        /// <summary>
        /// The underlying string value, unescaped
        /// </summary>
        public string Value { get; set; }

        #region IComparable methods and helpers
        public int CompareTo(BString other) => Value.CompareTo(other.Value);

        public override bool Equals(object obj)
        {
            return obj != null && obj is BString bs && bs.Value.Equals(Value);
        }

        public override int GetHashCode() => Value.GetHashCode();
        #endregion

        /// <summary>
        /// Escape the string specified IAW Json requirements, like replacing newlines
        /// with '\n', or things like '\u0003' and quotes.
        /// </summary>
        /// <param name="str">A string to be escaped.</param>
        /// <returns>An escaped string.</returns>
        internal static string Quote(string str)
        {
            var res = HttpUtility.JavaScriptStringEncode(str);
            return res;
        }

        public void BuildJson(StringBuilder sb)
        {
            _ = sb ?? throw new ArgumentNullException(nameof(sb));
            sb.Append('"').Append(Quote(Value)).Append('"');
        }

        public override string ToString() => Value;
    }
}
