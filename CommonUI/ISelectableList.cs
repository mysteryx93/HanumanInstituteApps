using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace HanumanInstitute.CommonUI
{
    /// <summary>
    /// Represents a list that can be bound to the UI with selection properties.
    /// </summary>
    /// <typeparam name="T">The type of item being shown in the list.</typeparam>
    public interface ISelectableList<T> : INotifyPropertyChanged
        where T : class
    {
        /// <summary>
        /// Gets or sets the list of items to display.
        /// </summary>
        ObservableCollection<T> List { get; }

        /// <summary>
        /// Replaces the content of the list with specified values.
        /// </summary>
        /// <param name="list">The list of values to add.</param>
        void ReplaceAll(IEnumerable<T> list);

        /// <summary>
        /// Gets or sets the selected index in the list. Set to -1 for no selection. Settings this property will also set SelectedItem to the corresponding value.
        /// </summary>
        int SelectedIndex { get; set; }

        void ForceSelect(int index);

        /// <summary>
        /// Gets the item currently selected in the list.
        /// </summary>
        T SelectedItem { get; }

        /// <summary>
        /// Gets whether the list has an item selected.
        /// </summary>
        bool HasSelection { get; }
    }
}
