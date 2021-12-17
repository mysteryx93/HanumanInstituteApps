using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace HanumanInstitute.Common.Avalonia;

/// <inheritdoc />
public class CollectionView<T> : ICollectionView<T>, INotifyPropertyChanged
{
    /// <summary>
    /// Initializes a new instance of the ListCollectionView class.
    /// </summary>
    public CollectionView()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ListCollectionView class, using a supplied collection that implements IList<typeparamref name="T"/>.
    /// </summary>
    /// <param name="list">The underlying collection, which must implement System.Collections.IList<typeparamref name="T"/>.</param>
    public CollectionView(IEnumerable<T> list)
    {
        Source.AddRange(list.ToList());
        CurrentItem = Source.FirstOrDefault()!;
    }

    /// <inheritdoc />
    public ObservableCollectionWithRange<T> Source { get; private set; } = new ObservableCollectionWithRange<T>();

    /// <inheritdoc />
    public T? CurrentItem
    {
        get => _currentItem;
        set
        {
            if (!Equals(value, _currentItem))
            {
                _currentItem = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentItem)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentPosition)));
            }
        }
    }
    private T? _currentItem;

    /// <inheritdoc />
    public int CurrentPosition => _currentItem != null ? Source.IndexOf(_currentItem) : -1;

    /// <inheritdoc />
    public void MoveCurrentToFirst() => CurrentItem = Source.FirstOrDefault();

    /// <inheritdoc />
    public void MoveCurrentToLast() => CurrentItem = Source.LastOrDefault();

    /// <inheritdoc />
    public void MoveCurrentToPosition(int position)
    {
        position = Math.Max(-1, Math.Min(Source.Count - 1, position));
        CurrentItem = position > -1 ? Source[position] : default;
    }

    /// <inheritdoc />
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        using var iterator = Source.GetEnumerator();
        while (iterator.MoveNext())
        {
            yield return iterator.Current!;
        }
    }

    /// <inheritdoc />
    public IEnumerator GetEnumerator()
    {
        using var iterator = Source.GetEnumerator();
        while (iterator.MoveNext())
        {
            yield return iterator.Current!;
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
