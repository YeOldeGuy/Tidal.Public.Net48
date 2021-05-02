using System;
using Prism.Mvvm;
using Tidal.Core.Models;
using ValidationModel;

namespace Tidal.Models
{
    public class TorrentSubFile : BindableBase
    {
        private bool _Wanted;
        private string _Name;
        private int _Index;
        private long _Length;

        public TorrentSubFile(string name, long length, int index)
        {
            Wanted = true;
            Name = name;
            Length = length;
            Index = index;
        }

        public bool Wanted { get => _Wanted; set => SetProperty(ref _Wanted, value); }

        public string Name { get => _Name; set => SetProperty(ref _Name, value); }

        public int Index { get => _Index; set => SetProperty(ref _Index, value); }

        public long Length { get => _Length; set => SetProperty(ref _Length, value); }
    }

    public class TorrentFileWanted : BindableBase, IDisposable
    {
        private bool _Wanted;
        private string _Name;
        private string _FilePath;
        private long _Size;
        private TorrentMetadata _Data;
        private ObservableItemCollection<TorrentSubFile> _Files;

        public TorrentFileWanted(string path, TorrentMetadata data)
        {
            FilePath = path;
            Data = data;
            Name = data.Info.Name;
            Size = data.Info.Length;
            Wanted = true;

            Files = new ObservableItemCollection<TorrentSubFile>();
            Files.ItemPropertyChanged += SubFilePropertyChanged;
            if (data.Info.Files != null)
            {
                int index = 0;
                foreach (var file in data.Info.Files)
                {
                    var individual = new TorrentSubFile(file.FullPath, file.Length, index);
                    Files.Add(individual);
                    index++;
                }
            }
        }

        private void SubFilePropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Files));
        }

        public string FilePath { get => _FilePath; set => SetProperty(ref _FilePath, value); }

        public bool Wanted
        {
            get => _Wanted;
            set => SetProperty(ref _Wanted, value);
        }

        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        public long Size
        {
            get => _Size;
            set => SetProperty(ref _Size, value);
        }

        public TorrentMetadata Data
        {
            get => _Data;
            set => SetProperty(ref _Data, value);
        }

        public ObservableItemCollection<TorrentSubFile> Files
        {
            get => _Files;
            set => SetProperty(ref _Files, value);
        }

        public void Dispose()
        {
            Files.ItemPropertyChanged -= SubFilePropertyChanged;
            Files.Dispose();
        }
    }
}
