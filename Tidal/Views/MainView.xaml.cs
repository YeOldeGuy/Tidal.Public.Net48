using Prism.Events;
using Prism.Regions;
using System.Windows.Controls;
using Tidal.Helpers;
using Tidal.Models.Messages;
using Tidal.Services.Abstract;

namespace Tidal.Views
{
    public partial class MainView : UserControl, INavigationAware
    {
        private ISettingsService settingsService;
        private SubscriptionToken saveToken;
        private readonly IMessenger Messenger;

        public MainView()
        {
            InitializeComponent();
            settingsService = ServiceResolver.Resolve<ISettingsService>();
            Messenger = ServiceResolver.Resolve<IMessenger>();
        }

        #region INavigationAware Methods
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            saveToken = Messenger.Subscribe<SaveSettingsMessage>(OnSaveSettings, ThreadOption.PublisherThread);

            settingsService = ServiceResolver.Resolve<ISettingsService>();

            var info = settingsService.MainPageLayout;
            if (info != null)
            {
                overall.RowDefinitions.Clear();
                foreach (var rowdef in info.GetLayoutGrid(nameof(overall)).LayoutSpecs)
                    overall.RowDefinitions.Add(rowdef.RowDefinition);

                details.ColumnDefinitions.Clear();
                foreach (var coldef in info.GetLayoutGrid(nameof(details)).LayoutSpecs)
                    details.ColumnDefinitions.Add(coldef.ColumnDefinition);
            }

            torrentGrid.Deserialize(settingsService.TorrentGridInfo);
            peerGrid.Deserialize(settingsService.PeerGridInfo);
            fileGrid.Deserialize(settingsService.FileGridInfo);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            saveToken.Dispose();
            SerializeLayoutGrid(settingsService);
            SerializeDataGrids(settingsService);
        }
        #endregion INavigationAware Methods

        #region Persistence Methods
        private void OnSaveSettings(SaveSettingsMessage saveMessage)
        {
            SerializeLayoutGrid(saveMessage.SettingsService);
            SerializeDataGrids(saveMessage.SettingsService);
        }

        private void SerializeDataGrids(ISettingsService settings)
        {
            settings.TorrentGridInfo = torrentGrid.Serialize();
            settings.PeerGridInfo = peerGrid.Serialize();
            settings.FileGridInfo = fileGrid.Serialize();
        }

        private void SerializeLayoutGrid(ISettingsService settings)
        {
            var info = new LayoutInfo();
            info.AddLayout(nameof(overall), overall.RowDefinitions);
            info.AddLayout(nameof(details), details.ColumnDefinitions);
            settings.MainPageLayout = info;
        }
        #endregion Persistence Methods
    }
}
