using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Tidal.Core.Helpers;

namespace Tidal.Converters
{
    public enum SizeSuffixes
    {
        Binary,     // Reports a value using IEC values, like "1.21GiB"
        Normal,     // Reports a value using traditional values, like "1.21GB"
    }


    /// <summary>
    /// Converts an numeric value to a human-readable string, presuming
    /// that the value is a size of a file or disk. The <see cref="Suffix"/>
    /// property determines whether the conversion is like "1.2GB" or "1.2GiB".
    /// </summary>
    public class SizeConverter : DependencyObject, IValueConverter
    {
        public SizeSuffixes Suffix
        {
            get { return (SizeSuffixes)GetValue(SuffixesProperty); }
            set { SetValue(SuffixesProperty, value); }
        }

        public static readonly DependencyProperty SuffixesProperty =
            DependencyProperty.Register(nameof(Suffix),
                                        typeof(SizeSuffixes),
                                        typeof(SizeConverter),
                                        new PropertyMetadata(SizeSuffixes.Normal));


        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var byteCount = System.Convert.ToInt64(value);
            if (byteCount == 0)
                return "0 Bytes";
            if (byteCount < 0)
                return "Error";

            int places = (parameter == null) ? 2 : int.Parse((string)parameter);

            return Suffix == SizeSuffixes.Normal ? byteCount.HumanSize(places) : byteCount.HumanSizeIEC(places);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
