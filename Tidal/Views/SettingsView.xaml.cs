using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Tidal.Helpers;
using Tidal.Models.BrokerMessages;
using Tidal.Services.Abstract;

namespace Tidal.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly ISettingsService settingsService;
        private readonly IMessenger Messenger;

        private readonly Uri downloadUri =
            new Uri("Views/SettingsPages/DownloadPage.xaml", UriKind.Relative);
        private readonly Uri applicationUri =
            new Uri("Views/SettingsPages/ApplicationPage.xaml", UriKind.Relative);
        private readonly Uri seedingUri =
            new Uri("Views/SettingsPages/SeedingPage.xaml", UriKind.Relative);
        private readonly Uri networkUri =
            new Uri("Views/SettingsPages/NetworkPage.xaml", UriKind.Relative);
        private readonly Uri restrictionsUri =
            new Uri("Views/SettingsPages/RestrictionsPage.xaml", UriKind.Relative);

        public SettingsView()
        {
            InitializeComponent();
            settingsService = ServiceResolver.Resolve<ISettingsService>();
            Messenger = ServiceResolver.Resolve<IMessenger>();

            contentFrame.Navigated += ContentFrame_Navigated;

            if (settingsService.SettingsPage == null)
                contentFrame.Navigate(downloadUri);
            else
                contentFrame.Navigate(settingsService.SettingsPage);

            Unloaded += PageUnloaded;
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            contentFrame.Navigated -= ContentFrame_Navigated;
            Unloaded -= PageUnloaded;
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Uri == downloadUri) downloading.IsChecked = true;
            else if (e.Uri == applicationUri) application.IsChecked = true;

            settingsService.SettingsPage = e.Uri;
        }

        private void Frame_LoadCompleted(object sender, NavigationEventArgs e)
        {
            SetFrameDataContext();
            Messenger.Send(new SessionRequest());
        }

        /*
       * The problem? How to have all of the separate frames (pages) share the
       * same view model as this user control, which gets its model from the
       * Prism library.
       * 
       * The answer comes from one Joe White back in 2010, via Stack Overflow.
       * See this: https://stackoverflow.com/a/3643716 
       * 
       * Basically, we're just watching for a frame to change and when it
       * does, grab the content as a FrameworkElement and set the DataContext.
       */
        private void Frame_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetFrameDataContext();
        }

        private void SetFrameDataContext()
        {
            if (contentFrame.Content is FrameworkElement content)
                content.DataContext = contentFrame.DataContext;
        }

        private void ApplicationPressed(object sender, RoutedEventArgs e)
        {
            contentFrame?.Navigate(applicationUri);
        }

        private void DownloadingPressed(object sender, RoutedEventArgs e)
        {
            contentFrame?.Navigate(downloadUri);
        }

        private void NetworkPressed(object sender, RoutedEventArgs e)
        {
            contentFrame?.Navigate(networkUri);
        }

        private void SeedingPressed(object sender, RoutedEventArgs e)
        {
            contentFrame?.Navigate(seedingUri);
        }

        private void RestrictionsPressed(object sender, RoutedEventArgs e)
        {
            contentFrame?.Navigate(restrictionsUri);
        }
    }
}
