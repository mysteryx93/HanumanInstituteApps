using System;
using System.Collections.Generic;

namespace HanumanInstitute.Common.Avalonia;

/// <summary>
/// Enables current record management on a collection.
/// </summary>
/// <typeparam name="T">The type of the list.</typeparam>
public interface ICollectionView<T> : IEnumerable<T>
{
    /// <summary>
    /// Gets the data source as an ObservableCollection. It uses a derived collection that supports range modifications.
    /// </summary>
    ObservableCollectionWithRange<T> Source { get; }
    /// <summary>
    /// Occurs when CurrentItem is changed.
    /// </summary>
    event EventHandler CurrentChanged;
    /// <summary>
    /// Gets the current item in the view.
    /// </summary>
    /// <returns>The current item of the view. By default, the first item of the collection starts as the current item.</returns>
    T? CurrentItem { get; set; }
    /// <summary>
    /// Gets or sets the position of the currently selected item, or -1 if none is selected.
    /// </summary>
    int CurrentPosition { get; }
    /// <summary>
    /// Moves selection to the first item in the list.
    /// </summary>
    void MoveCurrentToFirst();
    /// <summary>
    /// Moves selection to the last item in the list.
    /// </summary>
    void MoveCurrentToLast();
    /// <summary>
    /// Moves selection to specified position.
    /// </summary>
    /// <param name="position">The position to move the selection to.</param>
    void MoveCurrentToPosition(int position);
}
