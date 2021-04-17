using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tidal.Converters
{
    /// <summary>
    /// A special converter that takes a number and presents it as a percentage.
    /// Typical enough, but this one can take into account "close enough" (see
    /// the <see cref="Epsilon"/>) property. Also, values of 100% are reported
    /// without the decimal places.
    /// </summary>
    public class PercentConverter : DependencyObject, IValueConverter
    {
        /// <summary>
        /// Maximum value of the object being converted. Normally either 1.0 or 100.
        /// </summary>
        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        /// <summary>
        /// Minimum numeric value of the object being converted. Normally 0.0
        /// </summary>
        public double MinValue
        {
            get => (double)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        /// <summary>
        /// Value that determines "close enough" to 100 to count. Defaults to
        /// 0.01, which means that 99.99 will be reported as 100.
        /// </summary>
        public double Epsilon
        {
            get => (double)GetValue(EpsilonProperty);
            set => SetValue(EpsilonProperty, value);
        }

        /// <summary>
        /// Number of decimal places to report, such as 72.94 for two decimal
        /// places.
        /// </summary>
        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var divisor = MaxValue - MinValue;
            if (divisor <= 0)
                divisor = 1;

            var pct = System.Convert.ToDouble(value) * (100 / divisor);
            if (Math.Abs(100.0 - pct) < Epsilon)
                return "100%";

            return string.Format($"{{0:N{DecimalPlaces}}}%", pct);
        }

        // I could round-trip this, but it would be lossy.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();


        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(PercentConverter), new PropertyMetadata(1.0));
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(PercentConverter), new PropertyMetadata(0.0));
        public static readonly DependencyProperty EpsilonProperty =
           DependencyProperty.Register("Epsilon", typeof(double), typeof(PercentConverter), new PropertyMetadata(0.01));
        public static readonly DependencyProperty DecimalPlacesProperty =
            DependencyProperty.Register("DecimalPlaces", typeof(int), typeof(PercentConverter), new PropertyMetadata(1));
    }
}
