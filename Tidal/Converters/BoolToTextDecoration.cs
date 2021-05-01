using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tidal.Converters
{
    /// <summary>
    /// The various kinds of text decorations available.
    /// </summary>
    public enum DecorationKind
    {
        StrikeThrough,
        Underline,
        Overline,
        Baseline,
        None,
    }


    /// <summary>
    ///   Translates a boolean value to a <see cref="TextDecoration"/> for use
    ///   in TextBlocks.
    /// </summary>
    /// <remarks>
    ///   Typical usage (in the Resources section):
    /// <code>
    ///     &lt;converters:BoolToTextDecoration x:Key="BoolToStrikeThru"
    ///                                         Invert="True"
    ///                                         Kind="StrikeThrough" /&gt;
    /// </code>
    ///   Then, elsewhere, in a TextBlock:
    /// <code>
    ///   TextDecorations="{Binding Wanted,
    ///                             Converter={StaticResource BoolToStrikeThru}}"
    /// </code>
    ///   If "Wanted" is <see langword="false"/>, the text will have a strike through.
    /// </remarks>
    public class BoolToTextDecoration : DependencyObject, IValueConverter
    {
        public DecorationKind Kind
        {
            get => (DecorationKind)GetValue(KindProperty);
            set => SetValue(KindProperty, value);
        }
        public static readonly DependencyProperty KindProperty =
            DependencyProperty.Register(nameof(Kind),
                                        typeof(DecorationKind),
                                        typeof(BoolToTextDecoration),
                                        new PropertyMetadata(DecorationKind.StrikeThrough));


        /// <summary>
        /// If <see langword="true"/>, then invert the bound value to
        /// determine if the decoration should be applied.
        /// </summary>
        public bool Invert
        {
            get => (bool)GetValue(InvertProperty);
            set => SetValue(InvertProperty, value);
        }
        public static readonly DependencyProperty InvertProperty =
            DependencyProperty.Register(nameof(Invert),
                                        typeof(bool),
                                        typeof(BoolToTextDecoration),
                                        new PropertyMetadata(false));



        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TextDecorationCollection decoration;

            switch (Kind)
            {
                case DecorationKind.StrikeThrough:
                    decoration = TextDecorations.Strikethrough;
                    break;
                case DecorationKind.Underline:
                    decoration = TextDecorations.Underline;
                    break;
                case DecorationKind.Baseline:
                    decoration = TextDecorations.Baseline;
                    break;
                case DecorationKind.Overline:
                    decoration = TextDecorations.OverLine;
                    break;
                default:
                    decoration = null;
                    break;
            }
            if (value is bool b && (Invert ^ b))
                return decoration;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
