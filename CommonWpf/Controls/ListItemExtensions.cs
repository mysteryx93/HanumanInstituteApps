using System;
using System.Globalization;
using System.Collections.ObjectModel;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.CommonWpf
{
    public static class ListItemExtensions
    {
        /// <summary>
        /// Adds a new typed ListItem to the ObservableCollection.
        /// </summary>
        /// <typeparam name="T">The type of ListItem.</typeparam>
        /// <param name="list">The list to add the ListItem to.</param>
        /// <param name="value">The value of the new ListItem.</param>
        /// <param name="text">The text of the new ListItem.</param>
        /// <returns>The newly-created ListItem.</returns>
        public static ListItem<T> Add<T>(this ObservableCollection<ListItem<T>> list, T value, string text)
        {
            list.CheckNotNull(nameof(list));

            var item = new ListItem<T>(text, value);
            list.Add(item);
            return item;
        }

        /// <summary>
        /// Adds a new typed ListItem to the ObservableCollection. The text will be value.ToString().
        /// </summary>
        /// <typeparam name="T">The type of ListItem.</typeparam>
        /// <param name="list">The list to add the ListItem to.</param>
        /// <param name="value">The value of the new ListItem.</param>
        /// <returns>The newly-created ListItem.</returns>
        public static ListItem<T> Add<T>(this ObservableCollection<ListItem<T>> list, T value) => Add(list, value, Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
    }
}
