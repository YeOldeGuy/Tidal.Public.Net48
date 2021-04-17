using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tidal.Converters
{
    public class BoolToObjectConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty TrueValueProperty =
            DependencyProperty.Register(nameof(TrueValue),
                                        typeof(object),
                                        typeof(BoolToObjectConverter),
                                        new PropertyMetadata(null));

        public static readonly DependencyProperty FalseValueProperty =
            DependencyProperty.Register(nameof(FalseValue),
                                        typeof(object),
                                        typeof(BoolToObjectConverter),
                                        new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the value to be returned when the boolean is true
        /// </summary>
        public object TrueValue
        {
            get => GetValue(TrueValueProperty);
            set => SetValue(TrueValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the value to be returned when the boolean is false
        /// </summary>
        public object FalseValue
        {
            get => GetValue(FalseValueProperty);
            set => SetValue(FalseValueProperty, value);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return TrueValue;
            return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
