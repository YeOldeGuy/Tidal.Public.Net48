using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Tidal.Client.Contracts;

namespace Tidal.Client.Models
{
    public abstract class Assignable<T> : INotifyPropertyChanged, IAssignable<T>, ITag
        where T : INotifyPropertyChanged
    {
        #region ITag
        #region Backing Store
        private object _Tag;
        #endregion Backing Store

        [IgnoreDataMember]
        public object Tag
        {
            get => _Tag; set => SetProperty(ref _Tag, value);
        }
        #endregion ITag

        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }

        /// <summary>
        ///   Deriving classes should use this to raise events on property
        ///   change. Sets <see cref="IIsChanged.IsChanged"/> if implemented.
        /// </summary>
        /// <remarks>
        ///   Typical usage: <c>Set(ref MyBackingStore, newValue);</c>
        /// </remarks>
        /// <typeparam name="TProp">A reference type.</typeparam>
        /// <param name="storage">The property to set (needs getter and setter).</param>
        /// <param name="value">The value to set the field to.</param>
        /// <param name="propertyName">
        ///   Normally not necessary, the <see cref="nameof"/> value of the
        ///   property.
        /// </param>
        /// <returns>Returns <see langword="true"/> if the value was changed.</returns>
        protected bool SetProperty<TProp>(ref TProp storage, TProp value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            if (this is IIsChanged isChanged)
                isChanged.IsChanged = true;

            return true;
        }
        #endregion INPC

        #region IAssignable
        /// <summary>
        /// Assign one intance's values (usually the same type) to another
        /// instance, causing the <see cref="INotifyPropertyChanged"/> events to
        /// fire off for each change.
        /// </summary>
        /// <remarks>
        /// The <see cref="ITag.Tag"/> value is copied if <typeparamref
        /// name="T"/> implements <see cref="ITag"/>.
        /// </remarks>
        /// <param name="other"></param>
        public void Assign(T other)
        {
            AssignInternal(other);
            if (other is ITag tag)
                Tag = tag.Tag;
        }

        /// <summary>
        /// Does the actual leg work of the assignment in <see cref="Assign(T)"/>
        /// </summary>
        /// <param name="other">An instance whose values we covet.</param>
        protected abstract void AssignInternal(T other);
        #endregion IAssignable
    }
}
