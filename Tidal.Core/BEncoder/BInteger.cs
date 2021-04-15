using System;
using System.Text;

namespace Tidal.Core.BEncoder
{
    internal class BInteger : IBElement, IComparable<BInteger>
    {
        public BInteger(long value) { Value = value; }

        public long Value { get; set; }

        #region IComparable
        public int CompareTo(BInteger other) => Value.CompareTo(other.Value);

        public override bool Equals(object obj)
        {
            return obj != null && obj is BInteger b && b.Value.Equals(Value);
        }

        public override int GetHashCode() => Value.GetHashCode();
        #endregion

        public override string ToString() => Value.ToString();

        public void BuildJson(StringBuilder sb)
        {
            _ = sb ?? throw new ArgumentNullException(nameof(sb));
            sb.Append(Value);
        }
    }
}
