using System.Collections.Generic;
using Tidal.Client.Models;

namespace Tidal.Models.Messages
{
    /// <summary>
    /// A message from the view model to the view that asks for
    /// the currently selected <see cref="FileSummary"/> instances.
    /// </summary>
    class GetSelectedFilesMessage
    {
        /// <summary>
        /// Creates a new instance of <see cref="GetSelectedFilesMessage"/>,
        /// used to get the currently selected instances from the view.
        /// </summary>
        /// <param name="wanted">
        ///   The selected <see cref="FileSummary"/> instances should have the
        ///   <see cref="FileSummary.Wanted"/> set according to this value.
        /// </param>
        public GetSelectedFilesMessage(bool wanted)
        {
            SelectedFiles = null;
            Wanted = wanted;
        }

        /// <summary>
        /// A list of the selected instances of <see cref="FileSummary"/>,
        /// instantiated by the subscriber.
        /// </summary>
        public IList<FileSummary> SelectedFiles { get; set; }

        /// <summary>
        /// The value to set the <see cref="FileSummary.Wanted"/> property to.
        /// </summary>
        public bool Wanted { get; }
    }
}
