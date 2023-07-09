using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Enables collections to have the functionalities of current record management, custom sorting, filtering, and grouping.
    /// </summary>
    /// <typeparam name="T">The type of the list.</typeparam>
    public interface ICollectionView<T> : ICollectionView, IEnumerable<T>
    {
        IList<T> Source { get; }
        /// <summary>
        /// Gets the current item in the view.
        /// </summary>
        /// <returns>The current item of the view or null if there is no current item.</returns>
        new T CurrentItem { get; }
    }
}
