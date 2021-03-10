using Tidal.Services.Abstract;

namespace Tidal.Models.Messages
{
    /// <summary>
    /// A message to persist all of the subscriber's settings. THIS MUST
    /// BE SUBSCRIBED TO on <see cref="ThreadOption.PublisherThread"/>.
    /// </summary>
    internal class SaveSettingsMessage
    {
        /// <summary>
        /// Creates a message meant to tell any subscribers to save their
        /// settings to the provided <see cref="ISettingsService"/> instance.
        /// </summary>
        /// <param name="settingsService">The settings to write values to</param>
        public SaveSettingsMessage(ISettingsService settingsService)
        {
            SettingsService = settingsService;
        }

        /// <summary>
        /// The <see cref="ISettingsService"/> to use for writing values.
        /// </summary>
        public ISettingsService SettingsService { get; }
    }
}
