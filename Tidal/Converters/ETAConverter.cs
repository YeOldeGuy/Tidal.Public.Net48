using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Humanizer;

namespace Tidal.Converters
{
    public enum ETAForm
    {
        ETALongForm,
        ETAMediumForm,
        ETAShortForm,
        ETADateTimeForm,
    }

    /*
     * This converter will change a TimeSpan value into something
     * more legible to normal humans. By changing the declaration,
     * the value returned can be modified to be longer (more verbose)
     * or shorter (less verbose).
     *
     * Typical declarations:
     *
     * <conv:ETAConverter x:Key="MediumETAConverter" ETAForm="ETAMediumForm" />
     * <conv:ETAConverter x:Key="LongETAConverter" ETAForm="ETALongForm" />
     *
     * And typical usage:
     *
     *   <TextBlock Text="{Binding Torrent.ETA, Converter={StaticResource LongETAConverter}}" />
     *
     * There is, admittedly, not much difference between the Medium and Long
     * forms. The Short form is much more brief.
     */
    public class ETAConverter : DependencyObject, IValueConverter
    {
        public ETAForm ETAForm
        {
            get { return (ETAForm)GetValue(ETAFormProperty); }
            set { SetValue(ETAFormProperty, value); }
        }
        public static readonly DependencyProperty ETAFormProperty =
            DependencyProperty.Register(nameof(ETAForm), typeof(ETAForm), typeof(ETAConverter),
                new PropertyMetadata(ETAForm.ETAShortForm));


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan t)
            {
                switch (ETAForm)
                {
                    case ETAForm.ETAShortForm:
                        return ShortForm(t);
                    case ETAForm.ETAMediumForm:
                        return MediumForm(t);
                    case ETAForm.ETALongForm:
                        return LongForm(t);
                    case ETAForm.ETADateTimeForm:
                        if (t.Seconds < 0)
                            return "Unknown";
                        if (parameter != null && parameter is string fmt)
                        {
                            DateTime d = DateTime.Now + t;
                            return d.ToString(fmt);
                        }
                        break;
                }
            }
            return "";
        }

        private struct TimeParts
        {
            public int Years;
            public int Months;
            public int Weeks;
            public int Days;
            public int Hours;
            public int Minutes;
            public int Seconds;
        }

        private const double daysPerYear = 365.2425;
        private const double daysPerMonth = daysPerYear / 12;

        private static TimeParts BreakDownTime(TimeSpan t)
        {
            int years = (int)(t.Days / daysPerYear);
            if (years > 0)
                t -= TimeSpan.FromDays((int)(years * daysPerYear));
            int months = (int)(t.TotalDays / daysPerMonth);
            if (months > 0)
                t -= TimeSpan.FromDays((int)(months * daysPerMonth));
            int weeks = (int)(t.TotalDays / 7);
            if (weeks == 4)
            {
                weeks = 0;
                months++;
            }
            else if (weeks > 0)
                t -= TimeSpan.FromDays(weeks * 7);

            int days = t.Days;
            int hours = t.Hours;
            int minutes = t.Minutes;
            int seconds = t.Seconds;

            return new TimeParts
            {
                Years = years,
                Months = months,
                Weeks = weeks,
                Days = days,
                Hours = hours,
                Minutes = minutes,
                Seconds = seconds
            };
        }

        private static string ShortForm(TimeSpan t)
        {
            if (t.Seconds == -1)
                return "Unlimited";
            if (t.Seconds <= -2)
                return "Inactive";

            var parts = BreakDownTime(t);
            return parts.Months >= 1
                ? $"> " + "Mo".ToQuantity(parts.Months)
                : parts.Weeks >= 1
                ? $"> " + "Wk".ToQuantity(parts.Weeks)
                : parts.Days > 0
                ? $"{parts.Days}d {parts.Hours}h"
                : parts.Hours <= 0
                ? $"{parts.Minutes}m {parts.Seconds}s"
                : $"{parts.Hours}h {parts.Minutes}m";
        }

        private static string MediumForm(TimeSpan t)
        {
            if (t.Seconds == -1)
                return "Unlimited, Seeding";
            if (t.Seconds <= -2)
                return "No activity";

            var parts = BreakDownTime(t);
            if (parts.Months >= 1)
            {
                return $"More than {"Month".ToQuantity(parts.Months)}";
            }
            else if (parts.Weeks >= 1)
            {
                return $"More than {"Week".ToQuantity(parts.Weeks)}";
            }
            else if (parts.Days > 0)
            {
                return $"{"Day".ToQuantity(parts.Days)} {"Hour".ToQuantity(parts.Hours)}";
            }
            else
            {
                string hours = "Hour".ToQuantity(parts.Hours);
                string minutes = "Min".ToQuantity(parts.Minutes);
                string seconds = "Sec".ToQuantity(parts.Seconds);
                return $"{hours} {minutes} {seconds}";
            }
        }

        private static object LongForm(TimeSpan t)
        {
            if (t.Seconds == -1)
                return "Unlimited, Seeding";
            if (t.Seconds <= -2)
                return "No activity";

            var parts = BreakDownTime(t);

            if (parts.Months >= 1)
            {
                return $"Morethan {"Month".ToQuantity(parts.Months)}";
            }
            else if (parts.Weeks >= 1)
            {
                return $"More than {"Week".ToQuantity(parts.Weeks)}";
            }
            else if (parts.Days > 0)
            {
                return $"{"Day".ToQuantity(parts.Days)} {"Hour".ToQuantity(parts.Hours)}";
            }
            else
            {
                string hours = "Hour".ToQuantity(parts.Hours);
                string minutes = "Minute".ToQuantity(parts.Minutes);
                string seconds = "Second".ToQuantity(parts.Seconds);
                return $"{hours}, {minutes}, {seconds}";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
