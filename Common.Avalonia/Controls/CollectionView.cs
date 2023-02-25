using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace HanumanInstitute.Common.Avalonia;

/// <inheritdoc />
public class CollectionView<T> : ICollectionView<T>
{
    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

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
    public ObservableCollectionWithRange<T> Source { get; } = new();
    
    private void Source_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
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

    /// <inheritdoc />
    public void Add(T item) => Source.Add(item);

    /// <inheritdoc />
    int IList.Add(object? value)
    {
        try
        {
            Source.Add((T)value!);
            return Count - 1;
        }
        catch
        {
            return -1;
        }
    } 
    
    /// <inheritdoc cref="IList"/>
    public void Clear() => Source.Clear();

    /// <inheritdoc />
    void IList.Insert(int index, object? value) => Source.Insert(index, (T)value!);

    /// <inheritdoc />
    public bool Contains(T item) => Source.Contains(item);

    /// <inheritdoc />
    bool IList.Contains(object? value) => Source.Contains((T)value!);

    /// <inheritdoc />
    public void CopyTo(T[] array, int arrayIndex) => Source.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    void ICollection.CopyTo(Array array, int index) => Source.CopyTo((T[])array, index);

    /// <inheritdoc />
    public bool Remove(T item) => Source.Remove(item);

    /// <inheritdoc />
    void IList.Remove(object? value) => Source.Remove((T)value!);

    /// <inheritdoc cref="IList" />
    public void RemoveAt(int index) => Source.RemoveAt(index);

    /// <inheritdoc cref="IList" />
    public int Count => Source.Count;

    /// <inheritdoc />
    bool ICollection.IsSynchronized => ((ICollection)Source).IsSynchronized;
    
    /// <inheritdoc />
    object ICollection.SyncRoot => ((ICollection)Source).SyncRoot;

    /// <inheritdoc cref="IList" />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public int IndexOf(T item) => Source.IndexOf(item);

    /// <inheritdoc />
    public int IndexOf(object? value) => Source.IndexOf((T)value!);

    /// <inheritdoc />
    public void Insert(int index, T item) => Source.Insert(index, item);

    /// <inheritdoc />
    public bool IsFixedSize => false;

    /// <inheritdoc />
    public T this[int index]
    {
        get => Source[index];
        set => Source[index] = value;
    }

    /// <inheritdoc />
    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = (T)value!;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
