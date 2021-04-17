using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Tidal.Core.Helpers;

namespace Tidal.Converters
{
    public class SpeedConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            var byteCount = System.Convert.ToInt64(value);
            int places = (parameter == null) ? -1 : int.Parse((string)parameter);

            if (byteCount < 0)
                return InfiniteValue;

            return byteCount.HumanSpeed(places);
        }


        public string InfiniteValue
        {
            get => (string)GetValue(InfiniteValueProperty);
            set => SetValue(InfiniteValueProperty, value);
        }
        public static readonly DependencyProperty InfiniteValueProperty =
            DependencyProperty.Register(nameof(InfiniteValue), typeof(string),
                typeof(SpeedConverter), new PropertyMetadata("∞ Bps"));


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
