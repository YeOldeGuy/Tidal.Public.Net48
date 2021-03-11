using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tidal.Converters
{
    /*
     * Another simple converter, taking a DateTime value as its argument and a
     * format as its parameter. The difference between this and a call to
     * StringFormat is that the Unix epoch is taken into account. If the Unix
     * time is less or equal to zero, then the value of "ZeroTime" is returned
     * (defaults to: "Never").
     */
    public class DateFormatConverter : DependencyObject, IValueConverter
    {
        public string ZeroTime
        {
            get { return (string)GetValue(ZeroTimeProperty); }
            set { SetValue(ZeroTimeProperty, value); }
        }
        public static readonly DependencyProperty ZeroTimeProperty =
            DependencyProperty.Register(nameof(ZeroTime), typeof(string),
                typeof(DateFormatConverter), new PropertyMetadata("Never"));

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return ZeroTime;

            if (parameter == null)
                parameter = "G";

            if (value is DateTime d)
            {
                DateTimeOffset dto = DateTime.SpecifyKind(d, DateTimeKind.Utc);
                if (dto.ToUnixTimeSeconds() <= 0)
                    return ZeroTime;
                else
                    return d.ToString(parameter.ToString());
            }

            return "Error";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
