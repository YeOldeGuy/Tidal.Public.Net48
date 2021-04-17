using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Tidal.Converters
{
    /*
     * This converter will change an integer or boolean value to
     * a Visibility value, set in the declaration.
     *
     * To use it, declare it in your App.xaml or resources, something like
     * this:
     *
     *    <converters:VisibilityConverter x:Key="VisibilityConverter"
     *                                    FalseVisibility="Collapsed"
     *                                    NullVisibility="Collapsed"
     *                                    TrueVisibility="Visible" />
     *
     * Then, use it like this:
     *
     *    <Rectangle Grid.RowSpan="2"
     *               Fill="{Binding Error,
     *                              Converter={StaticResource ErrorBrushConverter}}"
     *               Visibility="{Binding Error,
     *                                    Converter={StaticResource VisibilityConverter}}">
     *    </Rectangle>
     *
     * Note that the value passed can also be an integer, like in the example.
     * Non-zero values are interpreted as true. There is also an additional value
     * that determines what is returned when the value is NULL. This is also
     * the default value returned if the value passed in is not a boolean or
     * integer.
     */
    public class VisibilityConverter : DependencyObject, IValueConverter
    {
        public Visibility TrueVisibility
        {
            get => (Visibility)GetValue(TrueVisibilityProperty);
            set => SetValue(TrueVisibilityProperty, value);
        }
        public static readonly DependencyProperty TrueVisibilityProperty =
            DependencyProperty.Register("TrueVisibility", typeof(Visibility),
                typeof(VisibilityConverter), new PropertyMetadata(Visibility.Visible));


        public Visibility FalseVisibility
        {
            get => (Visibility)GetValue(FalseVisibilityProperty);
            set => SetValue(FalseVisibilityProperty, value);
        }
        public static readonly DependencyProperty FalseVisibilityProperty =
            DependencyProperty.Register("FalseVisibility", typeof(Visibility),
                typeof(VisibilityConverter), new PropertyMetadata(Visibility.Collapsed));


        public Visibility NullVisibility
        {
            get => (Visibility)GetValue(NullVisibilityProperty);
            set => SetValue(NullVisibilityProperty, value);
        }
        public static readonly DependencyProperty NullVisibilityProperty =
            DependencyProperty.Register(nameof(NullVisibility), typeof(Visibility),
                typeof(VisibilityConverter), new PropertyMetadata(Visibility.Collapsed));



        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IConvertible conv)
                return System.Convert.ToBoolean(conv) ? TrueVisibility : FalseVisibility;
            else
                return NullVisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
