using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Tidal.Core.Helpers
{
    /// <summary>
    /// Represents a boolean array of days of the week, from
    /// Sunday to Saturday, mapping back-and-forth between an
    /// integer type.
    /// </summary>
    public class DayMap : IEnumerable<bool>
    {
        private readonly bool[] days = new bool[7];

        /// <summary>
        /// Creates an empty map where all of the days are false.
        /// </summary>
        public DayMap()
        {
        }

        /// <summary>
        /// Copy-constructor creates a new <see cref="DayMap"/>, then
        /// copies the specified <see cref="DayMap"/> into it.
        /// </summary>
        /// <param name="other"></param>
        public DayMap(DayMap other)
        {
            for (int i = 0; i < 7; i++)
                days[i] = other[i];
        }

        /// <summary>
        /// Creates a <see cref="DayMap"/>, then initializes it from the
        /// bits in the integer specified.
        /// </summary>
        /// <param name="bits">An integer type, with bit values set for the
        /// various days of the week. Sunday is bit 0, Monday is bit 1, up
        /// to Saturday as bit 6.</param>
        public DayMap(int? bits)
        {
            if (bits.HasValue)
            {
                for (int i = 0; i < 7; i++)
                {
                    days[i] = ((bits & (1 << i)) != 0);
                }
            }
        }

        public DayMap(IList<bool> bits)
        {
            if (bits != null)
            {
                for (int i = 0; i < 7; i++)
                {
                    days[i] = bits.ElementAtOrDefault(i);
                }
            }
        }

        public DayMap Assign(int bits)
        {
            for (int i = 0; i < 7; i++)
            {
                days[i] = ((bits & (1 << i)) != 0);
            }
            return this;
        }

        /// <summary>
        /// Gets the truth value at the specified offset. The offset
        /// must be between 0 and 6, inclusive (e.g., days of the week).
        /// </summary>
        /// <param name="i">A value representing days of the week, 0 for Sunday, 6 for Saturday.</param>
        /// <returns></returns>
        public bool this[int i]
        {
            get { return days[i]; }
            set { days[i] = value; }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is DayMap dm))
            {
                return false;
            }

            return days.Equals(dm.days);
        }

        public override int GetHashCode()
        {
            return days.GetHashCode();
        }

        /// <summary>
        /// Return the bitmap as an integer.
        /// </summary>
        /// <param name="map">The <see cref="DayMap"/> to coerce.</param>
        public static explicit operator int(DayMap map)
        {
            int val = 0;

            for (int i = 0; i < 7; i++)
            {
                val |= (map[i] ? (1 << i) : 0);
            }
            return val;
        }

        /// <summary>
        /// Return the bitmap as an integer
        /// </summary>
        /// <returns></returns>
        public int AsInteger()
        {
            return (int)this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[");

            for (int i = 0; i < 7; i++)
                sb.Append(days[i] ? CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames[i] + "," : "");

            sb.Remove(sb.Length - 1, 1); // trailing comma
            sb.Append(']');

            return sb.ToString();
        }

        public IEnumerator<bool> GetEnumerator()
        {
            for (int i = 0; i < 7; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
