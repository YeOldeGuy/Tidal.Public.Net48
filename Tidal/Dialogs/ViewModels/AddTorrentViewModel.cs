using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Tidal.Core.Helpers;
using Tidal.Models;
using Tidal.Models.Messages;
using Tidal.Properties;
using Tidal.Services.Abstract;
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

        private readonly IMessenger messenger;
        private SubscriptionToken AddToken;
        private DelegateCommand<AddTorrentDisposition?> _CloseDialogCommand;
        private bool _IsValid;

        public AddTorrentViewModel(IMessenger messenger)
        {
            this.messenger = messenger;
        }

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
            AddToken.Dispose();
            Files.ItemPropertyChanged -= Files_ItemPropertyChanged;
            Files.Dispose();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            AddToken = messenger.Subscribe<StartupMessage>(OnStartupMessage);

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

        private void OnStartupMessage(StartupMessage startupMessage)
        {
            var path = startupMessage.Args[0];
            if (TorrentReader.TryParse(path, out var meta))
            {
                Files.Add(new TorrentFileWanted(path, meta));
            }
        }

        private bool inIPC = false;

        private void Files_ItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            if (inIPC || !(e.ChangedItem is TorrentFileWanted changedItem))
                return;

            inIPC = true;
            try
            {
                if (e.PropertyChangedArgs.PropertyName == nameof(TorrentFileWanted.Wanted))
                {
                    foreach (var f in changedItem.Files)
                        f.Wanted = changedItem.Wanted == true;
                }
                else if (e.PropertyChangedArgs.PropertyName == nameof(TorrentFileWanted.Files))
                {
                    if (changedItem.Files.All(t => t.Wanted))
                        changedItem.Wanted = true;
                    else if (changedItem.Files.All(t => !t.Wanted))
                        changedItem.Wanted = false;
                    else
                        changedItem.Wanted = true;
                }
            }
            finally
            {
                inIPC = false;
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
