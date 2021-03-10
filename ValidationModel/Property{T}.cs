using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ValidationModel
{
    public class Property<T> : IProperty<T>, INotifyPropertyChanged
    {
        public Property(string propertyName = null)
        {
            Errors.CollectionChanged += (s, e) => RaisePropertyChanged(nameof(IsValid));
            PropertyName = propertyName;
        }


        public event EventHandler ValueChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Revert() => Value = OriginalValue;

        public void MarkAsClean() => OriginalValue = Value;

        public string PropertyName { get; }

        public override string ToString() => Value?.ToString();

        public ObservableCollection<string> Errors { get; } = new ObservableCollection<string>();

        public bool IsValid => !Errors.Any();

        public bool IsDirty => Value == null ? OriginalValue != null : !Value.Equals(OriginalValue);

        T _Value = default;
        public T Value
        {
            get { return _Value; }
            set
            {
                if (!IsOriginalSet)
                    OriginalValue = value;
                Set(ref _Value, value);
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        bool _IsOriginalSet = false;
        public bool IsOriginalSet
        {
            get { return _IsOriginalSet; }
            private set { Set(ref _IsOriginalSet, value); }
        }

        T _OriginalValue = default;
        public T OriginalValue
        {
            get { return _OriginalValue; }
            set
            {
                IsOriginalSet = true;
                Set(ref _OriginalValue, value);
            }
        }

        private bool Set<V>(ref V storage, V value)
        {
            if (Equals(storage, value))
                return false;
            storage = value;
            RaisePropertyChanged(PropertyName);
            RaisePropertyChanged(nameof(IsDirty));
            return true;
        }

        public void RaisePropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? PropertyName));
        }
    }
}
