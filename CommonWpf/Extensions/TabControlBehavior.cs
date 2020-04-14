using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

// Simple solution. Renamed attached property to IsCached.
// https://stackoverflow.com/a/44606372/3960200
// License: Attribution-ShareAlike 3.0 Unported https://creativecommons.org/licenses/by-sa/3.0/
//
// This other more complex solution seems to achieve the same results.
// https://www.codeproject.com/Articles/460989/WPF-TabControl-Turning-Off-Tab-Virtualization

namespace HanumanInstitute.CommonWpf {
    /// <summary>
    /// Wraps tab item contents in UserControl to prevent TabControl from re-using its content
    /// </summary>
    public class TabControlBehavior {
        private static readonly HashSet<TabControl> _tabControls = new HashSet<TabControl>();
        private static readonly Dictionary<ItemCollection, TabControl> _tabControlItemCollections = new Dictionary<ItemCollection, TabControl>();

        public static bool GetIsCached(TabControl tabControl) {
            return (bool)tabControl.GetValue(IsCachedProperty);
        }

        public static void SetIsCached(TabControl tabControl, bool value) {
            tabControl.SetValue(IsCachedProperty, value);
        }

        public static readonly DependencyProperty IsCachedProperty = DependencyProperty.RegisterAttached(
            "IsCached",
            typeof(bool),
            typeof(TabControlBehavior),
            new UIPropertyMetadata(false, OnIsCachedChanged));

        private static void OnIsCachedChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e) {
            var tabControl = depObj as TabControl;
            if (null == tabControl)
                return;
            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
                Attach(tabControl);
            else
                Detach(tabControl);
        }

        private static void Attach(TabControl tabControl) {
            if (!_tabControls.Add(tabControl))
                return;
            _tabControlItemCollections.Add(tabControl.Items, tabControl);
            ((INotifyCollectionChanged)tabControl.Items).CollectionChanged += TabControlUcWrapperBehavior_CollectionChanged;
        }

        private static void Detach(TabControl tabControl) {
            if (!_tabControls.Remove(tabControl))
                return;
            _tabControlItemCollections.Remove(tabControl.Items);
            ((INotifyCollectionChanged)tabControl.Items).CollectionChanged -= TabControlUcWrapperBehavior_CollectionChanged;
        }

        private static void TabControlUcWrapperBehavior_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            var itemCollection = (ItemCollection)sender;
            var tabControl = _tabControlItemCollections[itemCollection];
            IList items;
            if (e.Action == NotifyCollectionChangedAction.Reset) {   /* our ObservableArray<T> swops out the whole collection */
                items = (ItemCollection)sender;
            } else {
                if (e.Action != NotifyCollectionChangedAction.Add)
                    return;

                items = e.NewItems;
            }

            foreach (var newItem in items) {
                var ti = tabControl.ItemContainerGenerator.ContainerFromItem(newItem) as TabItem;
                if (ti != null) {
                    var userControl = ti.Content as UserControl;
                    if (null == userControl)
                        ti.Content = new UserControl { Content = ti.Content };
                }
            }
        }
    }
}