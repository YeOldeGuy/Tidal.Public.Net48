using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tidal.Converters
{
    /// <summary>
    ///   Translates a boolean value to a <see cref="TextDecoration"/> for use
    ///   in TextBlocks. Boolean value is inverted, assuming no decoration for
    ///   <see langword="true"/> values, and the specified decoration for <see
    ///   langword="false"/>.
    /// </summary>
    /// <remarks>
    ///   Typical usage:
    ///   <code>
    ///   TextDecorations="{Binding Wanted,
    ///                             ConverterParameter='StrikeThrough',
    ///                             Converter={StaticResource
    ///                             BoolToTextDecoration}}"
    ///   </code>
    ///   If "Wanted" is <see langword="false"/>, the text will have a strike through.
    /// </remarks>
    public class BoolToTextDecoration : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TextDecorationCollection decoration = TextDecorations.Strikethrough;

            if (parameter is string s)
            {
                switch (s.ToLower())
                {
                    case "strikethrough":
                    case "strikethru":
                        decoration = TextDecorations.Strikethrough;
                        break;
                    case "underline":
                    case "underscore":
                        decoration = TextDecorations.Underline;
                        break;
                    case "overline":
                        decoration = TextDecorations.OverLine;
                        break;
                    case "baseline":
                        decoration = TextDecorations.Baseline;
                        break;
                }
            }
            if (value is bool b && !b)
                return decoration;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
