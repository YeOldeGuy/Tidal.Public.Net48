using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Tidal.Models;
using Tidal.Properties;
using ValidationModel;

namespace Tidal.Dialogs.ViewModels
{
    public class AddTorrentInfo
    {
        public string Path { get; set; }
        public int[] UnwantedIndexes { get; set; }

        public override string ToString()
        {
            var indexes = string.Join(",", UnwantedIndexes.Select(p => p.ToString()).ToArray());
            return $"{Path}: [{indexes}]";
        }
    }


    public class AddTorrentViewModel : BindableBase, IDialogAware
    {
        // Not user visible. Do not translate
        public const string ActionParameter = "action";
        public const string PathParameter = "path";
        public const string MetaParameter = "meta";
        public const string FilesParameter = "files";
        public const string IsValidParameter = "isvalid";
        public const string UnwantedParameter = "unwanted";

        private DelegateCommand<AddTorrentDisposition?> _CloseDialogCommand;
        private bool _IsValid;

        private ObservableItemCollection<TorrentFileWanted> _Files;
        public ObservableItemCollection<TorrentFileWanted> Files
        {
            get => _Files;
            set => SetProperty(ref _Files, value);
        }

        public string Title => Resources.AddTorrentTitle;

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;


        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public void OnDialogClosed()
        {
            Files.ItemPropertyChanged -= Files_ItemPropertyChanged;
            Files.Dispose();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Files = new ObservableItemCollection<TorrentFileWanted>();
            Files.ItemPropertyChanged += Files_ItemPropertyChanged;

            var files = parameters.GetValue<IEnumerable<TorrentFileWanted>>(MetaParameter);
            if (files != null)
            {
                foreach (var file in files)
                    Files.Add(file);
                IsValid = true;
            }
            CloseDialogCommand.RaiseCanExecuteChanged();
        }

        private void Files_ItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            if (!(e.ChangedItem is TorrentFileWanted changedItem))
                return;

            if (e.PropertyChangedArgs.PropertyName == nameof(TorrentFileWanted.Files))
            {
                changedItem.Wanted = !changedItem.Files.All(t => !t.Wanted);
            }
        }

        public bool IsValid { get => _IsValid; set => SetProperty(ref _IsValid, value); }

        private IEnumerable<AddTorrentInfo> GetTorrentsInfo()
        {
            foreach (var file in Files)
            {
                if (!file.Wanted)
                    continue;

                var info = new AddTorrentInfo()
                {
                    Path = file.FilePath,
                    UnwantedIndexes = file.Files.Where(f => !f.Wanted).Select(g => g.Index).ToArray(),
                };
                yield return info;
            }
        }

        protected virtual void CloseDialog(AddTorrentDisposition? parameter)
        {
            ButtonResult result = ButtonResult.Cancel;
            if (parameter == AddTorrentDisposition.Pause || parameter == AddTorrentDisposition.Start)
            {
                result = ButtonResult.OK;
            }
            if (result == ButtonResult.Cancel)
            {
                RequestClose(new DialogResult(result));
            }
            else
            {
                // Watch out! If you don't convert to a list here, thereby
                // grabbing the information, there won't be anything in the
                // enumerable because the "Files" gets disposed of:
                var files = GetTorrentsInfo().ToList();

                var parameters = new DialogParameters()
                {
                    { ActionParameter, parameter },
                    { FilesParameter, files },
                };
                RequestClose(new DialogResult(result, parameters));
            }
        }

        #region Commands
        public DelegateCommand<AddTorrentDisposition?> CloseDialogCommand
            => _CloseDialogCommand = _CloseDialogCommand ?? new DelegateCommand<AddTorrentDisposition?>((p) =>
            {
                CloseDialog(p);
            }, (p) => p == AddTorrentDisposition.Cancel || IsValid);
        #endregion
    }
}
