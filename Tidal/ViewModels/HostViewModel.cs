using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Humanizer;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Tidal.Collections;
using Tidal.Models;
using Tidal.Models.Messages;
using Tidal.Properties;
using Tidal.Services.Abstract;
using ValidationModel;

namespace Tidal.ViewModels
{
    class HostViewModel : BindableBase, INavigationAware
    {
        #region Backing Store
        private string _Title = "hosts page";
        private ObservableItemCollection<Host> _Hosts;
        private Host _SelectedHost;
        private bool _IsOpen;
        private DelegateCommand _AddHost;
        private DelegateCommand _SaveHosts;
        private DelegateCommand _RemoveHost;
        private DelegateCommand _RevertChanges;
        private DelegateCommand _ActivateHost;
        #endregion

        private readonly IHostService hostService;
        private readonly IMessenger Messenger;
        private List<IDisposable> disposables;
        private List<Host> revertList;
        private bool listChanged;
        private Host activeHostAtStart;


        public HostViewModel(IHostService hostService, IMessenger messenger)
        {
            this.hostService = hostService;
            this.Messenger = messenger;
        }

        #region Properties Visible to XAML
        public ObservableItemCollection<Host> Hosts { get => _Hosts; set => SetProperty(ref _Hosts, value); }
        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        public bool IsHostSelected => SelectedHost != null;
        public bool IsOpen { get => _IsOpen; set => SetProperty(ref _IsOpen, value); }

        public bool IsDirty
        {
            get
            {
                if (Hosts == null)
                    return false;

                var dirty = Hosts.Any(s => s.IsDirty) || (Hosts.Count != revertList?.Count);
                return dirty;
            }
        }

        public Host SelectedHost
        {
            get => _SelectedHost;
            set
            {
                if (SetProperty(ref _SelectedHost, value))
                {
                    RaisePropertyChanged(nameof(IsHostSelected));
                    RaiseStatusChanged();
                }
            }
        }
        #endregion

        #region INavigationAware Methods
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            IsOpen = true;

            disposables = new List<IDisposable>();

            revertList = new List<Host>();
            Hosts = new MergeCollection<Host>();
            Hosts.ItemPropertyChanged += Hosts_ItemPropertyChanged;

            foreach (var host in hostService.Hosts)
            {
                Hosts.Add(host.Clone());
                revertList.Add(host.Clone());
            }

            var sel = Hosts.FirstOrDefault(a => a.Active);
            if (sel != null)
            {
                SelectedHost = sel;
                SelectedHost.MarkAsClean();
            }

            if (!Hosts.Any())
            {
                AddHost.Execute();
                Title = Resources.HostVM_NewHostAdded;
                Hosts.First().Active = true;
            }
            else
                SetTitle();

            listChanged = false;
            RaiseStatusChanged();

            disposables.Add(Messenger.Subscribe<SaveSettingsMessage>(OnSaveSettings, ThreadOption.PublisherThread));
            disposables.Add(Messenger.Subscribe<HaltMessage>(m => IsOpen = false));
            disposables.Add(Messenger.Subscribe<ResumeMessage>(m => IsOpen = true));

            activeHostAtStart = hostService.ActiveHost;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Hosts.ItemPropertyChanged -= Hosts_ItemPropertyChanged;

            Host active = Hosts.FirstOrDefault(h => h.Active);
            if (activeHostAtStart != active && active != null)
            {
                // Don't need to press the "Save" button to change the active host
                if (SaveHosts.CanExecute())
                    SaveHosts.Execute();
                Messenger.Send(new HostChangedMessage(active.Id));
            }

            Hosts.Dispose();
            foreach (var disposable in disposables)
                disposable.Dispose();
        }
        #endregion

        #region Helpers for Properties
        private void Hosts_ItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            RaiseStatusChanged();
        }

        private void OnSaveSettings(SaveSettingsMessage saveSettingsMessage)
        {
            if (SaveHosts.CanExecute())
                SaveHosts.Execute();
        }

        private void RaiseStatusChanged()
        {
            RaisePropertyChanged(nameof(IsDirty));
            RaisePropertyChanged(nameof(Hosts));

            ActivateHost.RaiseCanExecuteChanged();
            AddHost.RaiseCanExecuteChanged();
            RemoveHost.RaiseCanExecuteChanged();
            SaveHosts.RaiseCanExecuteChanged();
            RevertChanges.RaiseCanExecuteChanged();
        }

        private void SetTitle()
        {
            Title = "Host".ToQuantity(Hosts.Count, ShowQuantityAs.Words).ApplyCase(LetterCasing.Sentence);
        }
        #endregion

        #region Commands
        public DelegateCommand AddHost => _AddHost = _AddHost ?? new DelegateCommand(() =>
        {
            var host = new Host();
            Hosts.Add(host);
            SelectedHost = host;
            SetTitle();
            RaiseStatusChanged();
        }, () => true);

        public DelegateCommand ActivateHost => _ActivateHost = _ActivateHost ?? new DelegateCommand(() =>
        {
            foreach (var host in Hosts)
                host.Active = host == SelectedHost;
            RaiseStatusChanged();
        }, () => SelectedHost != null && SelectedHost.Active == false);

        public DelegateCommand RemoveHost => _RemoveHost = _RemoveHost ?? new DelegateCommand(() =>
        {
            Hosts.Remove(SelectedHost);
            SelectedHost = Hosts.FirstOrDefault();
            listChanged = true;
            SetTitle();
            RaiseStatusChanged();
        }, () => SelectedHost != null && !SelectedHost.Active);

        public DelegateCommand SaveHosts => _SaveHosts = _SaveHosts ?? new DelegateCommand(() =>
        {
            hostService.ReplaceAll(Hosts);
            hostService.ActiveHost = Hosts.FirstOrDefault(a => a.Active);
            hostService.Save();

            revertList.Clear();
            foreach (var acct in Hosts)
            {
                acct.MarkAsClean();
                revertList.Add(acct.Clone());
            }
            listChanged = false;
            Messenger.Send(new InfoMessage(Resources.HostVM_HostsSaved, null, TimeSpan.FromSeconds(3)));
            RaiseStatusChanged();
        }, () => IsDirty || listChanged);

        public DelegateCommand RevertChanges => _RevertChanges = _RevertChanges ?? new DelegateCommand(() =>
        {
            var selectedId = SelectedHost?.Id ?? Guid.Empty;
            Hosts.Clear();
            foreach (var acct in revertList)
            {
                Hosts.Add(acct.Clone());
            }

            SelectedHost = Hosts.FirstOrDefault(a => a.Id == selectedId);
            if (SelectedHost == null && Hosts.Any())
                SelectedHost = Hosts.First();

            listChanged = false;
            RaiseStatusChanged();
            SetTitle();
        }, () => IsDirty || listChanged);
        #endregion
    }
}
