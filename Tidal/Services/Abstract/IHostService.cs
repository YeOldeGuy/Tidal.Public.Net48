using System;
using System.Collections.Generic;
using Tidal.Models;

namespace Tidal.Services.Abstract
{
    public interface IHostService
    {
        /// <summary>
        /// A read-only list of the accounts as stored in the application
        /// persist. Recall that IReadOnly means that the order and length of
        /// the list is fixed; the elements are mutable. See the <see
        /// cref="ReplaceAll(IEnumerable{Host})"/> method for how to save any
        /// changes.
        /// </summary>
        IReadOnlyList<Host> Hosts { get; }

        /// <summary>
        /// Gets the active host or <see langword="null"/> if none
        /// is marked.
        /// </summary>
        Host ActiveHost { get; set; }

        Host GetHost(Guid id);

        /// <summary>
        /// Gets the number of <see cref="Host"/>s in the collection.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Replaces the Hosts with the new collection.
        /// </summary>
        /// <param name="hosts"></param>
        void ReplaceAll(IEnumerable<Host> hosts);

        /// <summary>
        /// Write the values to the application persist.
        /// </summary>
        void Save();
    }
}
