using System;
using System.Collections.Generic;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Tidal.Client.Models;
using Tidal.Properties;

namespace Tidal.Dialogs.ViewModels
{
    public class RemoveTorrentViewModel : BindableBase, IDialogAware
    {
        public const string RemoveDataParameter = "removedata";
        public const string TorrentsParameter = "torrents";

        private string _Title = "";
        private List<Torrent> _Torrents;

        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        public List<Torrent> Torrents { get => _Torrents; set => SetProperty(ref _Torrents, value); }


        public event Action<IDialogResult> RequestClose;


        public void OnDialogOpened(IDialogParameters parameters)
        {
            Title = Resources.RemoveTorrents_Title;
            var torrents = parameters.GetValue<List<Torrent>>(TorrentsParameter);
            Torrents = new List<Torrent>(torrents);
        }

        public void OnDialogClosed()
        {
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        protected virtual void CloseDialog(TorrentDataDisposition? parameter)
        {
            ButtonResult result = parameter == TorrentDataDisposition.NoAction ? ButtonResult.Cancel : ButtonResult.OK;
            DialogParameters parms = new DialogParameters()
            {
                { RemoveDataParameter, parameter },
            };

            RaiseRequestClose(new DialogResult(result, parms));
        }

        private DelegateCommand<TorrentDataDisposition?> _CloseCommand;
        public DelegateCommand<TorrentDataDisposition?> CloseCommand =>
            _CloseCommand = _CloseCommand ?? new DelegateCommand<TorrentDataDisposition?>((p) =>
            {
                CloseDialog(p);
            });
    }
}
