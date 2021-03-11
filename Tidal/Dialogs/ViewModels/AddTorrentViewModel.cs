using System;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Tidal.Core.Models;
using Tidal.Models;
using Tidal.Properties;

namespace Tidal.Dialogs.ViewModels
{
    public class AddTorrentViewModel : BindableBase, IDialogAware
    {
        // Not user visible. Do not translate
        public const string ActionParameter = "action";
        public const string PathParameter = "path";
        public const string MetaParameter = "meta";
        public const string IsValidParameter = "isvalid";
        public const string UnwantedParameter = "unwanted";

        private DelegateCommand<AddTorrentDisposition?> _CloseDialogCommand;
        private string _FileHeader;
        private long _TotalSize;
        private bool _IsValid;
        private string _FileName;
        private ObservableCollection<TorrentFileWanted> _FilesInTorrent;
        private string _TorrentPath;

        // This view model is associated with the dialog's view over in
        // the App.xaml.cs code, in the RegisterTypes() method. Just a
        // simple call.
        //
        // There are five things that need to be here to satisfy the
        // IDialogAware interface.
        //
        // 1. Title            This is what's shown in the dialog window title bar
        // 2. RequestClose     This is an event listened to by the dialog service
        // 3. CanCloseDialog   A method returning true if the dialog is closable
        // 4. OnDialogOpened   Like INavigationAware's OnNavigatedTo
        // 5. OnDialogClosed   Like INavigationAware's OnNavigatedFrom
        //
        // The first is easy. Specify a title you'd like the dialog window
        // to have. Remember, that unless overridden, the window will be a
        // standard window with the standard chrome.
        public string Title => Resources.AddTorrentTitle;

        // The second, the RequestClose event is as easy as declaring it, but
        // more about that next. This event must be raised to get the dialog
        // service to consider closing the window containing your dialog.
        public event Action<IDialogResult> RequestClose;

        // Thirdly, unless you have a dialog that needs for a certain condition
        // to be fulfilled prior to dismissing it, just return true here. When
        // you raise RequestClose, the dialog service calls this to see if it's
        // okay to close the window.
        public bool CanCloseDialog() => true;

        // The fourth requirement is implementing this method, OnDialogOpened.
        // It's invoked by the dialog service after the dialog has been
        // realized. If the dialog expects parameters, passed from a view model
        // somewhere, then this is where you take note of them, setting the
        // dialog up according to the data.
        public void OnDialogOpened(IDialogParameters parameters)
        {
            FilesInTorrent = new ObservableCollection<TorrentFileWanted>();
            TorrentPath = parameters.GetValue<string>(PathParameter);
            DisplayMetadata(parameters.GetValue<TorrentMetadata>(MetaParameter));
            IsValid = parameters.GetValue<bool>(IsValidParameter);
        }

        // The final requirement is to implement this method. When the dialog is
        // dismissed, no matter what the value of DialogResult, this method is
        // invoked. If you've allocated an IDisposable or similar, this is where
        // you should Dismiss() it.
        public void OnDialogClosed()
        {
        }

        // This is the simple code that we invoke when the dialog should be
        // closed. I don't think the RequestClose event will ever be null, but
        // we check anyway.
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }


        // The rest of this view model is the standard stuff, providing values
        // to display in the dialog.


        public bool IsValid { get => _IsValid; set => SetProperty(ref _IsValid, value); }

        public string FileName { get => _FileName; set => SetProperty(ref _FileName, value); }

        /// <summary>
        /// A string to display either "Folder Name" or "File Name" depending
        /// on the type of torrent.
        /// </summary>
        public string FileHeader
        {
            get => _FileHeader;
            set => SetProperty(ref _FileHeader, value);
        }

        /// <summary>
        /// The total size of the torrent file. Determined by the
        /// <see cref="DisplayMetadata(TorrentMetadata)"/> method.
        /// </summary>
        public long TotalSize
        {
            get => _TotalSize;
            set => SetProperty(ref _TotalSize, value);
        }

        // These are the files contained in the torrent. Each has a property
        // that determines whether the file should be downloaded initially
        // or not (is it wanted?). The user can change only that value of the
        // file.
        public ObservableCollection<TorrentFileWanted> FilesInTorrent
        {
            get => _FilesInTorrent;
            set => SetProperty(ref _FilesInTorrent, value);
        }

        /// <summary>
        /// The full path name of the .torrent file
        /// </summary>
        public string TorrentPath
        {
            get => _TorrentPath;
            set
            {
                _TorrentPath = value;
                CloseDialogCommand.RaiseCanExecuteChanged();
            }
        }

        private void DisplayMetadata(TorrentMetadata metaFile)
        {
            FilesInTorrent.Clear();
            if (metaFile == null)
            {
                TotalSize = 0;
                IsValid = false;
                FileName = string.Format(Resources.AddTorrent_CannotParse_1, TorrentPath);
                FileHeader = Resources.AddTorrent_BadFile;
            }
            else
            {
                if (metaFile.Info.Files != null)
                {
                    FileName = metaFile.Info.Name;
                    for (int i = 0; i < metaFile.Info.Files.Count; i++)
                    {
                        FilesInTorrent.Add(new TorrentFileWanted(i, metaFile.Info.Files[i]));
                    }
                    FileHeader = Resources.AddTorrent_FolderName;
                }
                else
                {
                    FileName = metaFile.Info.Name;
                    FilesInTorrent.Add(new TorrentFileWanted(FileName, metaFile.Info.Length));
                    FileHeader = Resources.AddTorrent_FileName;
                }
                TotalSize = metaFile.Info.Length;
                IsValid = true;
            }

            CloseDialogCommand.RaiseCanExecuteChanged();
        }

        protected virtual void CloseDialog(AddTorrentDisposition? parameter)
        {
            ButtonResult result = ButtonResult.Cancel;
            if (parameter == AddTorrentDisposition.Pause || parameter == AddTorrentDisposition.Start)
                result = ButtonResult.OK;

            if (result == ButtonResult.Cancel)
            {
                RequestClose(new DialogResult(result));
            }
            else
            {
                var parms = new DialogParameters()
                {
                    { ActionParameter, parameter },
                    { PathParameter, TorrentPath },
                    { UnwantedParameter, FilesInTorrent.Where(f => !f.Wanted).Select(f => f.Index) },
                };
                RaiseRequestClose(new DialogResult(result, parms));
            }
        }

        public DelegateCommand<AddTorrentDisposition?> CloseDialogCommand
            => _CloseDialogCommand = _CloseDialogCommand ?? new DelegateCommand<AddTorrentDisposition?>((disposition) =>
            {
                CloseDialog(disposition);
            }, (disposition) => disposition == AddTorrentDisposition.Cancel || IsValid);

    }
}
