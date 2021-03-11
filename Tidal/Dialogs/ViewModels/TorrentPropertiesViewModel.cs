﻿using System;
using System.Collections.Generic;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Tidal.Client.Models;
using Tidal.Models.BrokerMessages;
using Tidal.Models.Messages;
using Tidal.Properties;
using Tidal.Services.Abstract;

namespace Tidal.Dialogs.ViewModels
{
    public enum TorrentPropsDisposition
    {
        OK,
        Cancel,
    }


    class TorrentPropertiesViewModel : BindableBase, IDialogAware
    {
        private readonly IMessenger messenger;
        private Torrent _Torrent;
        private string _Title;
        private bool _SeedIdleModeChangable;
        private bool _SeedRatioModeChangable;
        private DelegateCommand<TorrentPropsDisposition> _CloseCommand;


        public const string TorrentParameter = "torrent";

        public TorrentPropertiesViewModel(IMessenger messenger)
        {
            this.messenger = messenger;
        }


        #region IDialogAware
        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public void OnDialogClosed()
        {
            Torrent.PropertyChanged -= Torrent_PropertyChanged;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Torrent = Torrent ?? new Torrent();
            Torrent.PropertyChanged += Torrent_PropertyChanged;
            if (parameters.ContainsKey(TorrentParameter))
            {
                var torrentParameter = parameters.GetValue<Torrent>(TorrentParameter);
                if (torrentParameter is null)
                    return;
                Torrent.Assign(torrentParameter);
                Title = Torrent.Name;
            }
        }
        #endregion

        #region Public Properties
        public List<string> SeedIdleSettings { get; } =
            new List<string>
            {
                Resources.TorrentProps_IdleGlobal,
                Resources.TorrentProps_IdleTorrent,
                Resources.TorrentProps_IdleUnlimited,
            };

        /// <summary>
        ///  Corresponds to the Combobox selector for the seed idle mode
        /// </summary>
        public int SeedIdleSelectedIndex
        {
            get => (int)Torrent.SeedIdleMode;
            set
            {
                Torrent.SeedIdleMode = (SeedLimitMode)value;
                SeedIdleModeChangable = Torrent.SeedIdleMode == SeedLimitMode.OverrideGlobalSettings;
            }
        }

        /// <summary>
        /// True if the mode selected allows for adjustment; the "This Torrent"
        /// mode.
        /// </summary>
        public bool SeedIdleModeChangable
        {
            get => _SeedIdleModeChangable;
            set => SetProperty(ref _SeedIdleModeChangable, value);
        }

        /// <summary>
        ///  Corresponds to the Combobox selector for the seed ratio mode
        /// </summary>
        public int SeedRatioSelectedIndex
        {
            get => (int)Torrent.SeedRatioMode;
            set
            {
                Torrent.SeedRatioMode = (SeedLimitMode)value;
                SeedRatioModeChangable = Torrent.SeedRatioMode == SeedLimitMode.OverrideGlobalSettings;
            }
        }

        /// <summary>
        /// True if the seed ratio mode selected allows for adjustment; the
        /// "This Torrent" mode.
        /// </summary>
        public bool SeedRatioModeChangable
        {
            get => _SeedRatioModeChangable;
            set => SetProperty(ref _SeedRatioModeChangable, value);
        }


        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        public Torrent Torrent { get => _Torrent; set => SetProperty(ref _Torrent, value); }
        #endregion

        private void Torrent_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Torrent torrent)
            {
                var info = torrent.GetType().GetProperty(e.PropertyName);

                var mutator = new TorrentMutator(e.PropertyName, info.GetValue(torrent));
                messenger.Send(new StatusInfoMessage($"Setting {e.PropertyName}", TimeSpan.FromSeconds(1)));
                messenger.Send(new SetTorrentsRequest(torrent.Id, mutator));
            }
        }

        protected virtual void CloseDialog(TorrentPropsDisposition disposition)
        {
            ButtonResult result = disposition == TorrentPropsDisposition.OK ? ButtonResult.OK : ButtonResult.Cancel;
            RaiseRequestClose(new DialogResult(result));
        }

        public DelegateCommand<TorrentPropsDisposition> CloseCommand =>
            _CloseCommand = _CloseCommand ?? new DelegateCommand<TorrentPropsDisposition>((p) =>
            {
                CloseDialog(p);
            }, (p) => true);
    }
}
