using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Tidal.Client.Contracts;
using ValidationModel;

namespace Tidal.Collections
{
    /// <summary>
    ///   A special collection that merges additions in a way that triggers
    ///   notifications when an element already exists.
    /// </summary>
    /// <remarks>
    ///   Maintains a collection of <typeparamref name="T"/>. When a new
    ///   collection is merged in, the resultant collection will match the new
    ///   collection; new items are added, existing items are modified so that
    ///   the property notifications are triggered, and the remainder are
    ///   removed.
    /// </remarks>
    /// <typeparam name="T">
    ///   A class type implementing <see cref="INotifyPropertyChanged"/>, <see
    ///   cref="IAssignable{T}"/>, <see cref="ITag"/>, and <see
    ///   cref="IEquatable{T}"/> interfaces.
    /// </typeparam>
    public class MergeCollection<T> : ObservableItemCollection<T>
        where T : class, INotifyPropertyChanged, IAssignable<T>, ITag, IEquatable<T>
    {
        // What we do is keep a set around for fast lookup. Every element of the
        // collection is also maintained in this dict, which makes the discovery
        // of new elements much quicker that having to do a FirstOrDefault() on
        // the list, over and over. 
        private readonly HashSet<T> lookupSet;


        /// <summary>
        /// Create a new <see cref="MergeCollection{T}"./>
        /// </summary>
        public MergeCollection()
        {
            lookupSet = new HashSet<T>();
        }

        /// <summary>
        /// Invoked prior to <see cref="Merge(IEnumerable{T})"/> beginning its
        /// work. Override this to do any last-minute massaging of the data.
        /// </summary>
        protected virtual void BeforeMerge(IEnumerable<T> newData) { }

        /// <summary>
        /// Called after all inserts and deletions have occurred, just
        /// prior to exiting the <see cref="Merge(IEnumerable{T})"/> method.
        /// </summary>
        /// <param name="rems">A collection of the elements that were removed.</param>
        /// 
        protected virtual void AfterMerge(IEnumerable<T> rems) { }

        protected override void ClearItems()
        {
            base.ClearItems();
            lookupSet.Clear();
        }

        /// <summary>
        ///   Make the collection reflect the new data, adding any new items and
        ///   overwriting any existing ones. Items in the collection that are
        ///   not in the new data are removed.
        /// </summary>
        /// <param name="newData">A collection of elements to merge.</param>
        /// <returns>
        ///   A collection of the elements that were removed upon merging.
        /// </returns>
        public IEnumerable<T> Merge(IEnumerable<T> newData)
        {
            if (newData == null)
                return null;

            // Do any last-minute massaging of the new data
            BeforeMerge(newData);

            // Use the Tag property to store a boolean marking if the 
            // element is to be removed.
            //
            // Assume that all the elements are going to be removed, which
            // is not atypical, actually. Peers can go from like 37 to 0 in
            // one update.
            foreach (var item in Items)
                item.Tag = false;

            // Look at each element in the new data. The element must
            // be in the newData to be kept.
            foreach (var element in newData)
            {
                element.Tag = true;

                // See if the new element is already in our data
                if (lookupSet.TryGetValue(element, out var f))
                {
                    // if it is, assign the element's values to the copy we have
                    f.Assign(element);
                }
                else
                {
                    // otherwise, add it to the data and our lookup table
                    Add(element);
                    lookupSet.Add(element);
                }
            }

            // Find all of the elements that didn't have the Tag value toggled,
            // meaning that they're to be deleted.
            var removals = Items.Where(t => (bool)t.Tag == false).ToList();
            foreach (var rem in removals)
            {
                Remove(rem);
                lookupSet.Remove(rem);
            }

            AfterMerge(removals);

            return removals;
        }

        public override string ToString()
        {
            var sb = new StringBuilder($"Count: {Count};");
            for (int i = 0; i < 3; i++)
                sb.Append($" {Items[i]},");
            if (Count >= 3)
                sb.Append("...");
            return sb.ToString();
        }
    }
}
