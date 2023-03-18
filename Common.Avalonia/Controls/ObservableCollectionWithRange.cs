using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;

namespace HanumanInstitute.Common.Avalonia;

public class ObservableCollectionWithRange<T> : ObservableCollection<T>
{
    public void AddRange(IEnumerable<T> items)
    {
        this.CheckReentrancy();
        foreach (var item in items)
        {
            this.Items.Add(item);
        }
        this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
