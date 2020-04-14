using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace HanumanInstitute.CommonUI
{
    /// <summary>
    /// Represents a list that can be bound to the UI with selection properties.
    /// </summary>
    /// <typeparam name="T">The type of item being shown in the list.</typeparam>
    public class SelectableList<T> : INotifyPropertyChanged, ISelectableList<T> where T : class
    {
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        /// <summary>
        /// Gets or sets the list of items to display.
        /// </summary>
        public ObservableCollection<T> List { get; } = new ObservableCollection<T>();

        /// <summary>
        /// Replaces the content of the list with specified values.
        /// </summary>
        /// <param name="list">The list of values to add.</param>
        public void ReplaceAll(IEnumerable<T> list)
        {
            List.Clear();
            if (list != null)
            {
                foreach (var item in list)
                {
                    List.Add(item);
                }
            }
        }

        /// <summary>
        /// Selects the item at specific index and populates SelectedItem property.
        /// </summary>
        /// <param name="index">The index of the item to select.</param>
        /// <param name="force">If true, an item will be selected when index is out of range, otherwise, it will be set to -1.</param>
        public void Select(int index, bool force = false)
        {
            if (force && List.Any())
            {
                index = index.Clamp(0, List.Count - 1);
            }

            if (index < 0 || index >= List.Count)
            {
                selectedIndex = -1;
                SelectedItem = null;
            }
            else
            {
                selectedIndex = index;
                SelectedItem = List[index];
            }
            HasSelection = SelectedItem != null;
        }

        /// <summary>
        /// Gets or sets the selected index in the list. Set to -1 for no selection. Settings this property will also set SelectedItem to the corresponding value.
        /// </summary>
        public int SelectedIndex {
            get => selectedIndex;
            set => Select(value, false);
        }
        private int selectedIndex = -1;

        /// <summary>
        /// Gets the item currently selected in the list.
        /// </summary>
        public T SelectedItem { get; private set; }

        /// <summary>
        /// Gets whether the list has an item selected.
        /// </summary>
        public bool HasSelection { get; private set; }
    }
}
