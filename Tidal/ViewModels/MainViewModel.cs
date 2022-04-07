﻿using Humanizer;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tidal.Client.Models;
using Tidal.Collections;
using Tidal.Constants;
using Tidal.Dialogs.ViewModels;
using Tidal.Models.BrokerMessages;
using Tidal.Models.Messages;
using Tidal.Services.Abstract;

namespace Tidal.ViewModels
{
    public class MainViewModel : BindableBase, INavigationAware
    {
        private readonly ISettingsService settingsService;
        private readonly IGeoService geoService;
        private readonly IDialogService dialogService;
        private readonly IMessenger messenger;

        private List<IDisposable> disposables;
        private List<Torrent> selectedTorrents;
        private bool needsSelectionsRefreshed;

        public MainViewModel(ISettingsService settingsService,
                             IGeoService geoService,
                             IMessenger messenger,
                             IDialogService dialogService)
        {
            this.settingsService = settingsService;
            this.geoService = geoService;
            this.dialogService = dialogService;
            this.messenger = messenger;
        }

        #region Properties Visible to XAML
        #region Backing Store
        private string _Title = "main page";
        private TorrentCollection _Torrents;
        private PeerCollection _Peers;
        private FileCollection _Files;
        #endregion Backing Store

        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        public TorrentCollection Torrents { get => _Torrents; set => SetProperty(ref _Torrents, value); }
        public PeerCollection Peers { get => _Peers; set => SetProperty(ref _Peers, value); }
        public FileCollection Files { get => _Files; set => SetProperty(ref _Files, value); }
        #endregion Properties Visible to XAML

        #region INavigationAware stuff
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            settingsService.SelectedHashes = selectedTorrents.ConvertAll(t => t.HashString);

            foreach (var disposable in disposables)
                disposable.Dispose();

            Files.ItemPropertyChanged -= Files_ItemPropertyChanged;
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            needsSelectionsRefreshed = true;

            disposables = new List<IDisposable>();
            selectedTorrents = new List<Torrent>();

            disposables.Add(Torrents = new TorrentCollection());
            disposables.Add(Peers = new PeerCollection(geoService));
            disposables.Add(Files = new FileCollection());
            disposables.Add(messenger.Subscribe<TorrentResponse>(OnTorrents));
            disposables.Add(messenger.Subscribe<SelectionUpdateMessage>(OnSelectedTorrentsChanged));
            disposables.Add(messenger.Subscribe<SaveSettingsMessage>(OnSaveSettings, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<HaltMessage>(OnHalt, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<ResumeMessage>(OnResume, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<HostChangedMessage>(OnHostChanged, ThreadOption.PublisherThread));
            disposables.Add(messenger.Subscribe<AddTorrentResponse>(OnAddTorrent));

            Files.ItemPropertyChanged += Files_ItemPropertyChanged;

            messenger.Send(new TorrentRequest());
        }

        private void Files_ItemPropertyChanged(object sender, ValidationModel.ItemPropertyChangedEventArgs e)
        {
            // In which we watch for change in the Files grid, looking for
            // when the user clicks on the Wanted checkbox. This lets the user
            // decide to download (or not) a specific file.

            if (e.PropertyChangedArgs.PropertyName != nameof(FileSummary.Wanted))
                return;

            if (e.ChangedItem is FileSummary summary)
            {
                var indexes = new List<int>();
                var msg = new GetSelectedFilesMessage(summary.Wanted);

                // Send a message to get the selected files. The subscriber must
                // fill the message instance with a list of them. What this is
                // for is so the user can select a bunch of files, click on one
                // of the "WANTED" checkboxes, and have *all* of those files
                // change their status to match.

                messenger.Send(msg); // two-way message to request selected files

                // If no one is subscribed to the message, the SelectedFiles
                // will come back unchanged, which is the null value. In that
                // case we simply use the FileSummary of the one chosen.

                if (msg.SelectedFiles is null)
                    indexes.Add(summary.Index);
                else
                    indexes.AddRange(msg.SelectedFiles.Select(f => f.Index));

                // Look for multiple torrents selected:
                var owners = msg.SelectedFiles.Select(f => f.OwnerId).Distinct();

                // Then, for each of the different torrents, send the signal to
                // change the state.
                foreach (var owner in owners)
                {
                    // This gets the indexes of the files belonging to a single
                    // torrent (owner, here):
                    var indexesOfOwner = msg.SelectedFiles.Where(o => o.OwnerId == owner)
                                                          .Select(f => f.Index)
                                                          .ToList();
                    var mutator = new TorrentMutator();
                    if (summary.Wanted)
                        mutator.FilesWanted = indexesOfOwner;
                    else
                        mutator.FilesUnwanted = indexesOfOwner;
                    messenger.Send(new SetTorrentsRequest(owner, mutator));
                }
            }
        }
        #endregion INavigationAware stuff

        #region Subscription Handlers
        private void ClearCollections()
        {
            Torrents.Clear();
            Peers.Clear();
            Files.Clear();
        }

        private void OnHalt(HaltMessage haltMessage)
        {
            ClearCollections();
        }

        private void OnHostChanged(HostChangedMessage hostChangedMessage)
        {
            ClearCollections();
        }

        private void OnResume(ResumeMessage resumeMessage)
        {
            needsSelectionsRefreshed = true;
            messenger.Send(new TorrentRequest());
        }

        private string addedTorrentHashString = string.Empty;

        private void OnTorrents(TorrentResponse torrentResponse)
        {
            if (Torrents == null || torrentResponse?.Torrents == null)
                return;

            Torrents.Merge(torrentResponse.Torrents);
            if (needsSelectionsRefreshed)
            {
                selectedTorrents.Clear();
                selectedTorrents.AddRange(from h in settingsService.SelectedHashes
                                          from t in Torrents
                                          where t.HashString == h || t.HashString == addedTorrentHashString
                                          select t);

                // persist the list again in case there's an added torrent in it now
                settingsService.SelectedHashes = selectedTorrents.ConvertAll(t => t.HashString);

                messenger.Send(new RestoreSelectionsMessage(settingsService.SelectedHashes));
                needsSelectionsRefreshed = false;
                addedTorrentHashString = string.Empty;
            }
            UpdateFromSelected();
        }

        private void OnAddTorrent(AddTorrentResponse addTorrentResponse)
        {
            if (addTorrentResponse.IsDuplicate)
                return;

            addedTorrentHashString = addTorrentResponse.HashString;
            needsSelectionsRefreshed = true;
        }

        private void OnSaveSettings(SaveSettingsMessage saveSettingsMessage)
        {
            settingsService.SelectedHashes = selectedTorrents.Select(t => t.HashString).ToList();
         }

        private void OnSelectedTorrentsChanged(SelectionUpdateMessage selectionUpdateMessage)
        {
            var hashes = selectionUpdateMessage.SelectedHashes;

            if (hashes.Any() == true)
            {
                var tors = from t in Torrents
                           from h in hashes
                           where t.HashString == h
                           select t;

                selectedTorrents = tors.ToList();
            }
            else
            {
                selectedTorrents.Clear();
            }
            UpdateFromSelected();
            RaiseCanExecuteChanged();
        }
        #endregion Subscription Handlers

        #region Helpers
        private void UpdateFromSelected()
        {
            var sels = selectedTorrents;
            if (Peers == null || Files == null)
                return;

            Peers.UpdateFromCollection(sels);
            Files.UpdateFromCollection(sels);

            SetTitle();

            RaiseCanExecuteChanged();
        }

        private void SetTitle()
        {
            StringBuilder titleBuilder = new StringBuilder();

            if (Torrents?.Any() != true)
            {
                Title = "No Torrents";
                return;
            }

            titleBuilder.Append("Torrent".ToQuantity(Torrents.Count, ShowQuantityAs.Words).Titleize())
                        .Append(", ")
                        .Append(selectedTorrents.Count.ToWords().Titleize())
                        .Append(" Selected");

            Title = titleBuilder.ToString();
        }

        private void RaiseCanExecuteChanged()
        {
            StartCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            RemoveTorrentsCommand.RaiseCanExecuteChanged();
            ReannounceCommand.RaiseCanExecuteChanged();
            QuickNormalCommand.RaiseCanExecuteChanged();
            QuickUnlimitedCommand.RaiseCanExecuteChanged();
        }
        #endregion Helpers

        #region ICommands
        #region Backing store for ICommands
        private DelegateCommand _StartCommand;
        private DelegateCommand _StopCommand;
        private DelegateCommand _ReannounceCommand;
        private DelegateCommand _QuickUnlimitedCommand;
        private DelegateCommand _QuickNormalCommand;
        private DelegateCommand _RemoveTorrentsCommand;
        #endregion Backing store for ICommands

        public DelegateCommand StartCommand =>
            _StartCommand = _StartCommand ?? new DelegateCommand(
                () => messenger.Send(new StartTorrentsRequest(selectedTorrents)),
                () => selectedTorrents?.Any(t => t.Status == TorrentStatus.Stopped) == true);

        public DelegateCommand StopCommand =>
            _StopCommand = _StopCommand ?? new DelegateCommand(
                () => messenger.Send(new StopTorrentsRequest(selectedTorrents)),
                () => selectedTorrents?.Any(t => t.Status != TorrentStatus.Stopped) == true);

        public DelegateCommand ReannounceCommand =>
            _ReannounceCommand = _ReannounceCommand ?? new DelegateCommand(
                () => messenger.Send(new ReannounceTorrentsRequest(selectedTorrents)),
                () => selectedTorrents?.Any(t => t.Status != TorrentStatus.Stopped) == true);

        public DelegateCommand QuickUnlimitedCommand =>
            _QuickUnlimitedCommand = _QuickUnlimitedCommand ?? new DelegateCommand(() =>
        {
            TorrentMutator mutator = new TorrentMutator()
            {
                SeedRatioMode = SeedLimitMode.Unlimited,
                SeedIdleMode = SeedLimitMode.Unlimited,
                Ids = selectedTorrents.ConvertAll(t => t.Id)
            };
            messenger.Send(new SetTorrentsRequest(mutator));
        }, () => selectedTorrents?.Any() == true);

        public DelegateCommand QuickNormalCommand =>
            _QuickNormalCommand = _QuickNormalCommand ?? new DelegateCommand(() =>
        {
            TorrentMutator mutator = new TorrentMutator()
            {
                SeedRatioMode = SeedLimitMode.FollowGlobalSettings,
                SeedIdleMode = SeedLimitMode.FollowGlobalSettings,
                Ids = selectedTorrents.ConvertAll(t => t.Id)
            };
            messenger.Send(new SetTorrentsRequest(mutator));
        }, () => selectedTorrents?.Any() == true);

        public DelegateCommand RemoveTorrentsCommand =>
            _RemoveTorrentsCommand = _RemoveTorrentsCommand ?? new DelegateCommand(() =>
        {
            IDialogParameters parameters = new DialogParameters
            {
                { RemoveTorrentViewModel.TorrentsParameter, selectedTorrents },
            };
            dialogService.ShowDialog(PageKeys.RemoveTorrents, parameters, r =>
            {
                // Closing the dialog by clicking the exit button on the title
                // bar will yield a ButtonResult.None.

                if (r.Result != ButtonResult.OK)
                    return;

                var removeData = r.Parameters.GetValue<TorrentDataDisposition?>(RemoveTorrentViewModel.RemoveDataParameter);
                messenger.Send(new RemoveTorrentsRequest(selectedTorrents, removeData == TorrentDataDisposition.RemoveData));
            });
        });

        private DelegateCommand _TorrentPropertiesCommand;
        public DelegateCommand TorrentPropertiesCommand =>
            _TorrentPropertiesCommand = _TorrentPropertiesCommand ?? new DelegateCommand(() =>
            {
                Torrent selected = selectedTorrents.FirstOrDefault();

                IDialogParameters parameters = new DialogParameters
                {
                    { TorrentPropertiesViewModel.TorrentParameter, selected },
                };
                dialogService.ShowDialog(PageKeys.TorrentProperties, parameters, _ =>
                {
                    // There isn't any OK returned from the properties dialog,
                    // it simply makes the changes as they're selected by the
                    // user. The only button is "Dismiss"
                });
            }, () => selectedTorrents?.Any() == true);
        #endregion ICommands
    }
}
