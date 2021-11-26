using System;
using System.Globalization;
using System.Windows.Data;

namespace Tidal.Converters
{
    internal class KConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int iVal)
            {
                if (iVal > 5000)
                    return "5K+";
                else if (iVal > 4000)
                    return "4K+";
                else if (iVal > 3000)
                    return "3K+";
                else if (iVal > 2000)
                    return "2K+";
                else if (iVal > 1000)
                    return "1K+";
                else
                    return iVal.ToString();
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
