using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Humanizer;

namespace Tidal.Converters
{
    public enum TimeSpanPresentation
    {
        Normal,     // produces something like "0.12:34:56.0172"
        Humanize,   // produces something line "12 hours, 35 minutes"
    }


    /// <summary>
    /// An <see cref="IValueConverter"/> to convert a <see cref="TimeSpan"/> to a
    /// <see cref="string"/>.
    /// </summary>
    public class TimeSpanConverter : DependencyObject, IValueConverter
    {

        /// <summary>
        /// Determines the way the <see cref="TimeSpan"/> is presented.
        /// </summary>
        public TimeSpanPresentation Presentation
        {
            get => (TimeSpanPresentation)GetValue(PresentationProperty);
            set => SetValue(PresentationProperty, value);
        }


        /// <summary>
        /// Determines how many values are returned. For a timespan of 01:23:45,
        /// precision of 1 will give "1 Hour". Precision 2 would return "1 Hour,
        /// 24 Minutes".
        /// </summary>
        public int Precision
        {
            get => (int)GetValue(PrecisionProperty);
            set => SetValue(PrecisionProperty, value);
        }

        /// <summary>
        /// A sort of Epsilon value, if a <see cref="TimeSpan"/>'s number of seconds is
        /// less than this, it will be considered zero.
        /// </summary>
        public double Tolerance
        {
            get => (double)GetValue(ToleranceProperty);
            set => SetValue(ToleranceProperty, value);
        }


        /// <summary>
        /// If a <see cref="TimeSpan"/> value is less than the <see cref="Tolerance"/>
        /// value, then this string will be returned.
        /// </summary>
        public string NearZero
        {
            get => (string)GetValue(NearZeroProperty);
            set => SetValue(NearZeroProperty, value);
        }



        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value.GetType().IsPrimitive)
            {
                var tsSeconds = System.Convert.ToInt64(value);
                value = TimeSpan.FromSeconds(tsSeconds);
            }

            if (value is TimeSpan ts)
            {
                if (ts.TotalSeconds <= Tolerance)
                    return NearZero;

                switch (Presentation)
                {
                    case TimeSpanPresentation.Normal:
                        return ts.ToString();
                    case TimeSpanPresentation.Humanize:
                        ts = new TimeSpan(ts.Days, ts.Hours, ts.Minutes, ts.Seconds);
                        return ts.Humanize(Precision, maxUnit: Humanizer.Localisation.TimeUnit.Day);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static readonly DependencyProperty ToleranceProperty =
            DependencyProperty.Register(nameof(Tolerance),
                                        typeof(double),
                                        typeof(TimeSpanConverter),
                                        new PropertyMetadata(3.0));

        public static readonly DependencyProperty PrecisionProperty =
            DependencyProperty.Register(nameof(Precision),
                                        typeof(int),
                                        typeof(TimeSpanConverter),
                                        new PropertyMetadata(2));

        public static readonly DependencyProperty NearZeroProperty =
            DependencyProperty.Register(nameof(NearZero),
                                        typeof(string),
                                        typeof(TimeSpanConverter),
                                        new PropertyMetadata("Just now"));

        public static readonly DependencyProperty PresentationProperty =
            DependencyProperty.Register(nameof(PresentationProperty),
                                        typeof(TimeSpanPresentation),
                                        typeof(TimeSpanConverter),
                                        new PropertyMetadata(TimeSpanPresentation.Humanize));
    }
}
