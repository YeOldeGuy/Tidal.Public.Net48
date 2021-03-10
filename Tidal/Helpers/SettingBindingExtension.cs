using System.Windows.Data;
using Tidal.Services.Abstract;

namespace Tidal.Helpers
{
    /// <summary>
    /// A binding that makes the elements of the <see cref="ISettingsService"/>
    /// visible to XAML.
    /// </summary>
    /// <remarks>
    /// Allows you to do things like:
    /// <code>
    ///  Width="{helpers:SettingBinding Width}"
    /// </code>
    /// where <c>Width</c> is a property in the settings.
    /// </remarks>
    public class SettingBindingExtension : Binding
    {
        public SettingBindingExtension()
        {
            Initialize();
        }

        public SettingBindingExtension(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            // Tie the extension to our settings
            Source = ServiceResolver.Resolve<ISettingsService>();

            // And make the connection two-way
            Mode = BindingMode.TwoWay;
        }
    }
}
