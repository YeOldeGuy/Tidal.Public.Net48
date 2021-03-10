using System.ComponentModel;

namespace Tidal.Client.Contracts
{
    /// <summary>
    ///   A contract for assigning one type to another, but in such a way as to
    ///   cause a bunch of <see cref="INotifyPropertyChanged"/> events for the
    ///   values that are differennt between the two.
    /// </summary>
    /// <typeparam name="T">
    ///   Any type implementing <see cref="INotifyPropertyChanged"/>.
    /// </typeparam>
    public interface IAssignable<in T>
        where T : INotifyPropertyChanged
    {
        /// <summary>
        /// Assign all the values in the <paramref name="other"/> instance to
        /// this instance. If <typeparamref name="T"/> implements the
        /// <see cref="IKeep"/> interface, the value of <see cref="IKeep.Keep"/>
        /// is also copied.
        /// </summary>
        /// <param name="other">Another instance of <typeparamref name="T"/></param>
        void Assign(T other);
    }
}
