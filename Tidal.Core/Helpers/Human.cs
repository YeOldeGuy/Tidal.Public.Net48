using Humanizer;
using System;
using System.Text;

namespace Tidal.Core.Helpers
{
    public static class Human
    {
        public static int SizeMultiplier { get; set; } = 1000;
        public static int SpeedMultiplier { get; set; } = 1000;
        public static int MemoryMultiplier { get; set; } = 1024;

        private static readonly string[] siSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        private static readonly string[] iecSuffixes = { "B", "KiB", "MiB", "GiB", "TiB", "PiB" };

        public static string HumanSpeed(this long speed, int decimalPlaces = 0)
        {
            if (speed <= 0)
                return "0 Bps";
            if (decimalPlaces < 0)
                decimalPlaces = 0;

            long bps = Math.Abs(speed);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bps, SpeedMultiplier)));

            // If the speed is less than 1MB/s, report only the whole numbers,
            // since those will be detailed enough.
            if (place < 2)
                decimalPlaces = 0;

            double num = Math.Round(bps / Math.Pow(SpeedMultiplier, place), decimalPlaces);

            num *= (speed < 0) ? -1 : 1;
            var s = num.ToString($"N{decimalPlaces}");
            return $"{s} {siSuffixes[place]}ps";
        }

        public static string HumanSpeed(this decimal speed, int decimalPlaces = 0)
        {
            return HumanSpeed((long)speed, decimalPlaces);
        }

        public static string HumanSpeed(this int speed, int decimalPlaces = 0)
        {
            return HumanSpeed((long)speed, decimalPlaces);
        }

        public static string HumanSize(this long size, int places = 2)
        {
            long byteCount = size;

            if (byteCount == 0)
                return "0 Bytes";

            if (byteCount < SizeMultiplier)
            {
                return $"{byteCount} Bytes";
            }

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, SizeMultiplier)));
            double num = Math.Round(bytes / Math.Pow(SizeMultiplier, place), places);

            double factor = byteCount < 0 ? -1 : 1;
            return string.Format("{0} {1}", (factor * num).ToString($"N{places}"), siSuffixes[place]);
        }

        public static string HumanSizeIEC(this long size, int places = 2)
        {
            long byteCount = size;

            if (byteCount == 0)
                return "0 Bytes";

            if (byteCount < 1024)
                return $"{byteCount} Bytes";

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), places);

            double factor = byteCount < 0 ? -1 : 1;
            return string.Format("{0} {1}", (factor * num).ToString($"N{places}"), iecSuffixes[place]);
        }


        public static string HumanTimeSpan(this TimeSpan t, bool verbose = false)
        {
            if (verbose)
            {
                return DateTime.UtcNow.Subtract(t).Humanize().Transform(To.SentenceCase);
            }

            if (t.Ticks < 0)
                return string.Empty;

            if (t.TotalDays > 365.25)
                return "> 1y";

            if (t.TotalDays > 30.5)
                return "> 1mo";

            if (t.TotalDays > 7)
            {
                int weeks = (int)t.TotalDays / 7;
                string plural = weeks == 1 ? "" : "s";
                return $"> {weeks}wk{plural}";
            }

            StringBuilder timeSpanBuilder = new StringBuilder(50);

            if (t.Days > 0)
                timeSpanBuilder.Append(t.Days).Append('d');

            if (t.Hours > 0)
                timeSpanBuilder.Append(t.Hours).Append('h');

            if (t.Days > 0 && t.Hours > 0)
                return timeSpanBuilder.ToString();

            if (t.Minutes > 0)
                timeSpanBuilder.Append(t.Minutes).Append('m');

            if (t.Hours > 0 && t.Minutes > 0)
                return timeSpanBuilder.ToString();

            if (t.Days <= 0 && t.Seconds != 0)
                timeSpanBuilder.Append(t.Seconds).Append('s');

            return timeSpanBuilder.ToString();
        }
    }
}
