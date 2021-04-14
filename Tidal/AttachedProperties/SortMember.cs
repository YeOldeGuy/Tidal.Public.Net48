using System.Windows;
using System.Windows.Controls;

namespace Tidal.AttachedProperties
{
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
