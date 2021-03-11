using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Tidal.Models;
using Tidal.Properties;

namespace Tidal.Dialogs.ViewModels
{
    public enum FirstHostDisposition
    {
        Add,
        Cancel,
    }

    public class FirstHostViewModel : BindableBase, IDialogAware
    {
        private Host _SelectedHost;
        private string _Title = Resources.FirstHostTitle;
        private DelegateCommand<FirstHostDisposition?> _CloseCommand;

        // Do not localize
        public const string HostParameter = "host";


        public string Title
        {
            get => _Title;
            set => SetProperty(ref _Title, value);
        }

        public Host SelectedHost
        {
            get => _SelectedHost;
            set => SetProperty(ref _SelectedHost, value);
        }


        public event Action<IDialogResult> RequestClose;

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
            // Check that a host has been entered?
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            SelectedHost = new Host()
            {
                Name = "TrueNAS Client",
                Address = "192.168.86.28", // the author's TrueNAS system
                Port = 9091,
                UseAuthentication = true,
                UserName = "transmission",
                Password = "daemon",
                Active = true,
            };
        }

        protected virtual void CloseDialog(FirstHostDisposition? disposition)
        {
            ButtonResult result = (disposition == FirstHostDisposition.Add) ? ButtonResult.OK : ButtonResult.Cancel;

            DialogParameters parameters = new DialogParameters()
            {
                { HostParameter, SelectedHost },
            };
            RaiseRequestClose(new DialogResult(result, parameters));
        }

        public DelegateCommand<FirstHostDisposition?> CloseCommand =>
            _CloseCommand = _CloseCommand ?? new DelegateCommand<FirstHostDisposition?>((p) =>
            {
                CloseDialog(p);
            }, (p) => true);
    }
}
