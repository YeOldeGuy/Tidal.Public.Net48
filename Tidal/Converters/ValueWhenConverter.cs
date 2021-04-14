using System;
using System.Globalization;
using System.Windows.Data;

namespace Tidal.Converters
{
    // Based on (stolen from)
    // https://github.com/Windows-XAML/Template10/blob/master/Source/Template10.Converters/ValueWhenConverter.cs
    //
    // Or, if that doesn't work, here's a TinyURL of a live, permalink:
    // https://tinyurl.com/y9r5mzw3
    //
    // Why'd you do it, guys? Why'd you give up on Template10? Folding it into
    // Prism hasn't worked; the UWP version of Prism, via the template studio,
    // uses version 6.3 of Prism. That's not exactly current and demonstrates,
    // at least to me, that while WPF, which everyone thought was dying, isn't,
    // and UWP is moribund. Even the devs working on Prism don't pay attention
    // to UWP.
    //  
    //
    // Example:
    //
    // <conv:ValueWhenConverter x:Key="VisibleWhenTrueConverter">
    //     <conv:ValueWhenConverter.When>
    //         <sys:Boolean>True</sys:Boolean>
    //     </conv:ValueWhenConverter.When>
    //     <conv:ValueWhenConverter.Value>
    //         <Visibility>Visible</Visibility>
    //     </conv:ValueWhenConverter.Value>
    //     <conv:ValueWhenConverter.Otherwise>
    //         <Visibility>Collapsed</Visibility>
    //     </conv:ValueWhenConverter.Otherwise>
    // </conv:ValueWhenConverter>
    //
    // Or:
    //
    // <conv:ValueWhenConverter x:Key="InvertBoolConverter">
    //     <conv:ValueWhenConverter.When>
    //         <sys:Boolean>True</sys:Boolean>
    //     </conv:ValueWhenConverter.When>
    //     <conv:ValueWhenConverter.Value>
    //         <sys:Boolean>False</sys:Boolean>
    //     </conv:ValueWhenConverter.Value>
    //     <conv:ValueWhenConverter.Otherwise>
    //         <sys:Boolean>True</sys:Boolean>
    //     </conv:ValueWhenConverter.Otherwise>
    // </conv:ValueWhenConverter>
    //

    public class ValueWhenConverter : IValueConverter
    {
        public object Value { get; set; }
        public object Otherwise { get; set; }
        public object When { get; set; }
        public object OtherwiseValueBack { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return Equals(value, parameter ?? When) ? Value : Otherwise;
            }
            catch
            {
                return Otherwise;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (OtherwiseValueBack is null)
            {
                throw new InvalidOperationException("Cannot ConvertBack without OtherwiseValueBack defined");
            }

            try
            {
                return Equals(value, Value) ? When : OtherwiseValueBack;
            }
            catch
            {
                return OtherwiseValueBack;
            }
        }
    }
}
