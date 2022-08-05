using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using JetBrains.Annotations;

namespace HanumanInstitute.Common.Avalonia;

/// <inheritdoc />
public class CollectionView<T> : ICollectionView<T>
{
    /// <summary>
    /// Initializes a new instance of the ListCollectionView class.
    /// </summary>
    public CollectionView()
    {
        Source.CollectionChanged += Source_CollectionChanged;
    }
    
    /// <summary>
    /// Initializes a new instance of the ListCollectionView class, using a supplied collection that implements IList<typeparamref name="T"/>.
    /// </summary>
    /// <param name="list">The underlying collection, which must implement System.Collections.IList<typeparamref name="T"/>.</param>
    public CollectionView(IEnumerable<T> list) : this()
    {
        Source.AddRange(list.ToList());
        CurrentItem = Source.FirstOrDefault()!;
    }

    /// <inheritdoc />
    public ObservableCollectionWithRange<T> Source { get; private set; } = new ObservableCollectionWithRange<T>();
    
    private void Source_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Must clear selection first otherwise removing last item doesn't select the new selection.
        var pos = CurrentPosition;
        _currentPosition = -1;
        OnPropertyChanged(nameof(CurrentPosition));
        OnPropertyChanged(nameof(CurrentItem));
        SetAndCoercePosition(pos);
    }

    /// <inheritdoc />
    public int CurrentPosition
    {
        get => _currentPosition;
        set => SetAndCoercePosition(value);
    }
    private int _currentPosition = -1;

    private void SetAndCoercePosition(int position)
    {
        if (position > -1 || _currentPosition > -1)
        {
            _currentPosition = Math.Max(-1, Math.Min(Source.Count - 1, position));
            OnPropertyChanged(nameof(CurrentPosition));
            OnPropertyChanged(nameof(CurrentItem));
        }
    }

    /// <inheritdoc />
    public T? CurrentItem
    {
        get => _currentPosition > -1 ? Source.ElementAtOrDefault(_currentPosition) : default;
        set => SetAndCoercePosition(value != null ? Source.IndexOf(value) : -1);
    }

    /// <inheritdoc />
    public void MoveCurrentToFirst() => CurrentItem = Source.FirstOrDefault();

    /// <inheritdoc />
    public void MoveCurrentToLast() => CurrentItem = Source.LastOrDefault();

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        using var iterator = Source.GetEnumerator();
        while (iterator.MoveNext())
        {
            yield return iterator.Current!;
        }
    }
    
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
