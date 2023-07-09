using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace HanumanInstitute.Avalonia;

/// <summary>
/// Enables current record management on a collection.
/// </summary>
/// <typeparam name="T">The type of the list.</typeparam>
public interface ICollectionView<T>  : INotifyPropertyChanged, INotifyCollectionChanged, IList<T>, IList
{
    /// <summary>
    /// Gets the data source as an ObservableCollection. It uses a derived collection that supports range modifications.
    /// </summary>
    ObservableCollectionWithRange<T> Source { get; }
    /// <summary>
    /// Gets the current item in the view.
    /// </summary>
    /// <returns>The current item of the view. By default, the first item of the collection starts as the current item.</returns>
    T? CurrentItem { get; }
    /// <summary>
    /// Gets or sets the position of the currently selected item, or -1 if none is selected.
    /// </summary>
    int CurrentPosition { get; set; }
    /// <summary>
    /// Moves selection to the first item in the list.
    /// </summary>
    void MoveCurrentToFirst();
    /// <summary>
    /// Moves selection to the last item in the list.
    /// </summary>
    void MoveCurrentToLast();
}
