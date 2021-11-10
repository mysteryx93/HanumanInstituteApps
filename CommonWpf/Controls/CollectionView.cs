using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Data;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Represents the generic collection view for collections that implement System.Collections.Generic.IList<T>.
    /// </summary>
    /// <typeparam name="T">The type of list.</typeparam>
    public class CollectionView<T> : CollectionView, ICollectionView<T>
    {
        public IList<T> Source { get; private set; }

        public CollectionView() : base(new List<T>())
        {
            Source = (IList<T>)base.SourceCollection;
        }

        /// <summary>
        /// Initializes a new instance of the ListCollectionView class, using a supplied collection that implements IList<typeparamref name="T"/>.
        /// </summary>
        /// <param name="list">The underlying collection, which must implement System.Collections.IList<typeparamref name="T"/>.</param>
        public CollectionView(IList<T> list) : base(list)
        {
            Source = list;
        }

        /// <summary>
        /// Returns an object that you can use to enumerate the items in the view.
        /// </summary>
        /// <returns>An System.Collections.IEnumerator<typeparamref name="T"/> object that you can use to enumerate the items in the view.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            var iterator = base.GetEnumerator();
            while (iterator.MoveNext())
            {
                yield return (T)iterator.Current!;
            }
        }

        /// <summary>
        /// Gets the current item in the view.
        /// </summary>
        /// <returns>The current item of the view. By default, the first item of the collection starts as the current item.</returns>
        public new T CurrentItem => (T)base.CurrentItem;
    }
}
