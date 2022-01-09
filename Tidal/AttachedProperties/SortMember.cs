using System.Windows;
using System.Windows.Controls;

namespace Tidal.AttachedProperties
{
    /// <summary>
    /// I don't know what this class is for, and at this point I'm afraid to
    /// ask. Why I didn't just use the "SortMember" property of data grid
    /// columns is escaping me at the time of writing.
    /// </summary>
    public class SortMember : DependencyObject
    {
        public static string GetName(DataGridColumn obj)
        {
            return (string)obj.GetValue(NameProperty);
        }

        public static void SetName(DataGridColumn obj, string value)
        {
            obj.SetValue(NameProperty, value);
        }


        public static readonly DependencyProperty NameProperty =
            DependencyProperty.RegisterAttached("Name",
                                                typeof(string),
                                                typeof(SortMember),
                                                new PropertyMetadata(""));
    }
}
