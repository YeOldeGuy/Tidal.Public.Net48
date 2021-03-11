using System;
using System.Globalization;
using System.Windows.Data;

namespace Tidal.Converters
{
    // Based on (stolen from)
    // https://github.com/Windows-XAML/Template10/blob/master/Source/Template10.Converters/ValueWhenConverter.cs
    //
    // That's a dead link now. Why'd you do it, guys? Why'd you give up on
    // Template10? Folding it into Prism hasn't worked; the UWP version of
    // Prism, via the template manager, uses version 6.3 of Prism. That's not
    // exactly current and demonstrates, at least to me, that while WPF, which
    // everyone thought was dying, isn't, and UWP is moribund. Even the devs
    // working on Prism don't pay attention to UWP.
    //  

    /*
     * Example:
     *
     * <conv:ValueWhenConverter x:Key="VisibleWhenTrueConverter">
     *     <conv:ValueWhenConverter.When>
     *         <sys:Boolean>True</sys:Boolean>
     *     </conv:ValueWhenConverter.When>
     *     <conv:ValueWhenConverter.Value>
     *         <Visibility>Visible</Visibility>
     *     </conv:ValueWhenConverter.Value>
     *     <conv:ValueWhenConverter.Otherwise>
     *         <Visibility>Collapsed</Visibility>
     *     </conv:ValueWhenConverter.Otherwise>
     * </conv:ValueWhenConverter>
     *
     * Or:
     *
     * <conv:ValueWhenConverter x:Key="InvertBoolConverter">
     *     <conv:ValueWhenConverter.When>
     *         <sys:Boolean>True</sys:Boolean>
     *     </conv:ValueWhenConverter.When>
     *     <conv:ValueWhenConverter.Value>
     *         <sys:Boolean>False</sys:Boolean>
     *     </conv:ValueWhenConverter.Value>
     *     <conv:ValueWhenConverter.Otherwise>
     *         <sys:Boolean>True</sys:Boolean>
     *     </conv:ValueWhenConverter.Otherwise>
     * </conv:ValueWhenConverter>
     */

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
                if (Equals(value, parameter ?? When))
                    return Value;
                return Otherwise;
            }
            catch
            {
                return Otherwise;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (OtherwiseValueBack == null)
                throw new InvalidOperationException("Cannot ConvertBack without OtherwiseValueBack defined");

            try
            {
                if (object.Equals(value, Value))
                    return When;
                return OtherwiseValueBack;
            }
            catch
            {
                return OtherwiseValueBack;
            }
        }
    }
}
