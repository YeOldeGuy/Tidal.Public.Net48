using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ValidationModel
{
    // Stolen from Template10:

    public class ItemPropertyChangedEventArgs : EventArgs
    {
        public ItemPropertyChangedEventArgs(object item, int changedIndex, PropertyChangedEventArgs e)
        {
            ChangedItem = item;
            ChangedItemIndex = changedIndex;
            PropertyChangedArgs = e;
        }
        public object ChangedItem { get; set; }

        public int ChangedItemIndex { get; set; }

        public PropertyChangedEventArgs PropertyChangedArgs { get; set; }
    }

    public class ObservableItemCollection<T> : ObservableCollection<T>, IDisposable
        where T : INotifyPropertyChanged
    {
        private bool _enableCollectionChanged = true;
        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        public event EventHandler<ItemPropertyChangedEventArgs> ItemPropertyChanged;

        public ObservableItemCollection()
        {
            base.CollectionChanged += (s, e) =>
            {
                if (_enableCollectionChanged)
                    CollectionChanged?.Invoke(this, e);
            };
        }

        public ObservableItemCollection(IEnumerable<T> collection) : base(collection)
        {
            base.CollectionChanged += (s, e) =>
            {
                if (_enableCollectionChanged)
                {
                    CollectionChanged?.Invoke(this, e);
                }
            };
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            CheckDisposed();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    RegisterPropertyChanged(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    UnRegisterPropertyChanged(e.OldItems);
                    if (e.NewItems != null)
                    {
                        RegisterPropertyChanged(e.NewItems);
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    break;
            }
            base.OnCollectionChanged(e);
        }

        private void RegisterPropertyChanged(IList items)
        {
            CheckDisposed();
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged += new PropertyChangedEventHandler(Item_PropertyChanged);
                }
            }
        }

        private void UnRegisterPropertyChanged(IList items)
        {
            CheckDisposed();
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged -= new PropertyChangedEventHandler(Item_PropertyChanged);
                }
            }
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            CheckDisposed();
            ItemPropertyChanged?.Invoke(this, new ItemPropertyChangedEventArgs(sender, IndexOf((T)sender), e));
        }


        public void AddRange(IEnumerable<T> items)
        {
            CheckDisposed();
            _enableCollectionChanged = false;
            foreach (var item in items)
            {
                Add(item);
            }
            _enableCollectionChanged = true;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items));
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            CheckDisposed();
            _enableCollectionChanged = false;
            foreach (var item in items)
            {
                Remove(item);
            }
            _enableCollectionChanged = true;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items));
        }

        protected override void ClearItems()
        {
            UnRegisterPropertyChanged(this);
            foreach (var item in Items)
            {
                if (item is IDisposable disposable)
                    disposable.Dispose();
            }

            base.ClearItems();
        }

        public void CheckDisposed()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ClearItems();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
