using System;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

namespace Tidal.Dialogs.ViewModels
{
    class RetryCancelViewModel : BindableBase, IDialogAware
    {
        public const string TitleParameter = "title";
        public const string MessageParameter = "message";
        public const string HeaderParameter = "header";


        private DelegateCommand<string> _CloseDialogCommand;
        private string _Header;
        private string _Message;
        private string _Title;


        public string Title { get => _Title; set => SetProperty(ref _Title, value); }
        public string Header { get => _Header; set => SetProperty(ref _Header, value); }
        public string Message { get => _Message; set => SetProperty(ref _Message, value); }


        public event Action<IDialogResult> RequestClose;

        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        protected virtual void CloseDialog(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "retry")
                result = ButtonResult.OK;
            else if (parameter?.ToLower() == "cancel")
                result = ButtonResult.Cancel;

            RaiseRequestClose(new DialogResult(result));
        }

        public DelegateCommand<string> CloseDialogCommand =>
            _CloseDialogCommand = _CloseDialogCommand ?? new DelegateCommand<string>((s) =>
            {
                CloseDialog(s);
            }, (s) => true);

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
            Title = parameters?.GetValue<string>(TitleParameter);
            Message = parameters?.GetValue<string>(MessageParameter);
            Header = parameters?.GetValue<string>(HeaderParameter);
        }
    }
}
