using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Tidal.Core.Helpers;
using Tidal.Properties;

namespace Tidal.Dialogs.ViewModels
{
    public class AddMagnetViewModel : BindableBase, IDialogAware
    {
        public const string ActionParameter = "action";
        public const string LinkParameter = "link";

        private string _Title;
        private string _DisplayName;
        private bool _IsValid;
        private string _MagnetUri;
        private DelegateCommand<AddTorrentDisposition?> _CloseDialogCommand;
        private ObservableCollection<string> _Trackers;
        public event Action<IDialogResult> RequestClose;

        // Please see the comments of the AddTorrentViewModel for a thorough
        // explanation of how the view model fits in with the Prism dialog
        // service.

        public string Title { get => _Title; set => SetProperty(ref _Title, value); }

        /// <summary>
        /// This is the name as extracted from the magnet link. It is rarely
        /// the actual file name to download.
        /// </summary>
        public string DisplayName
        {
            get => _DisplayName;
            private set => SetProperty(ref _DisplayName, value);
        }

        /// <summary>
        /// Set to <see langword="true"/> if the magnet link entered is valid.
        /// </summary>
        public bool IsValid
        {
            get => _IsValid;
            set => SetProperty(ref _IsValid, value);
        }

        public ObservableCollection<string> Trackers
        {
            get
            {
                if (_Trackers == null)
                    _Trackers = new ObservableCollection<string>();
                return _Trackers;
            }
            private set => _Trackers = value;
        }

        /// <summary>
        /// The magnet link displayed in the dialog. If it's valid, then the
        /// trakcers and display name will be updated.
        /// </summary>
        public string MagnetUri
        {
            get => _MagnetUri;
            set
            {
                if (SetProperty(ref _MagnetUri, value))
                {
                    IsValid = MagnetUtils.IsValidMagnetUrl(value, out var dict);
                    if (IsValid)
                        DisplayMagnetData(dict);
                    CloseDialogCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private void DisplayMagnetData(Dictionary<string, string> dict)
        {
            DisplayName = dict.ContainsKey("dn")
                ? Uri.UnescapeDataString(dict["dn"])
                : string.Empty;

            Trackers.Clear();
            if (dict.ContainsKey("tr"))
            {
                var tr = dict["tr"];
                var udps = tr.Split(',');
                foreach (var tracker in udps)
                    Trackers.Add(Uri.UnescapeDataString(tracker));
            }
        }

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            //
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            IsValid = false;
            Title = Resources.AddMagnetTitle;
            var link = Clipboard.GetText();
            if (MagnetUtils.IsValidMagnetUrl(link))
                MagnetUri = link;
        }


        protected virtual void CloseDialog(AddTorrentDisposition? parameter)
        {
            ButtonResult result = ButtonResult.Cancel;
            if (parameter == AddTorrentDisposition.Pause || parameter == AddTorrentDisposition.Start)
                result = ButtonResult.OK;

            var parms = new DialogParameters()
            {
                { ActionParameter, parameter },
                { LinkParameter, MagnetUri }
            };
            RaiseRequestClose(new DialogResult(result, parms));
        }

        public DelegateCommand<AddTorrentDisposition?> CloseDialogCommand =>
            _CloseDialogCommand = _CloseDialogCommand ?? new DelegateCommand<AddTorrentDisposition?>((disposition) =>
            {
                CloseDialog(disposition);
            }, (disposition) => disposition == AddTorrentDisposition.Cancel || IsValid);
    }
}
