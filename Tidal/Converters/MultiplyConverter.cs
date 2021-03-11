using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tidal.Converters
{
    class MultiplyConverter : DependencyObject, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double multiplicand = System.Convert.ToDouble(value);
            if (parameter != null)
            {
                double multiplier = System.Convert.ToDouble(parameter);
                return multiplicand * multiplier;
            }
            return multiplicand;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
