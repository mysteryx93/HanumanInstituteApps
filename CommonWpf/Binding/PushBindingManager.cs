using System;
using System.Windows;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Used by PushBinding.
    /// </summary>
    public static class PushBindingManager
    {
        public static readonly DependencyProperty PushBindingsProperty =
            DependencyProperty.RegisterAttached("PushBindingsInternal",
                                                typeof(PushBindingCollection),
                                                typeof(PushBindingManager),
                                                new UIPropertyMetadata(null));

        public static PushBindingCollection GetPushBindings(DependencyObject depObj)
        {
            if (depObj == null) { throw new ArgumentNullException(nameof(depObj)); }
            if (depObj.GetValue(PushBindingsProperty) == null)
            {
                depObj.SetValue(PushBindingsProperty, new PushBindingCollection(depObj));
            }
            return (PushBindingCollection)depObj.GetValue(PushBindingsProperty);
        }
        public static void SetPushBindings(DependencyObject depObj, PushBindingCollection value)
        {
            if (depObj == null) { throw new ArgumentNullException(nameof(depObj)); }
            depObj.SetValue(PushBindingsProperty, value);
        }


        public static readonly DependencyProperty StylePushBindingsProperty =
            DependencyProperty.RegisterAttached("StylePushBindings",
                                                typeof(PushBindingCollection),
                                                typeof(PushBindingManager),
                                                new UIPropertyMetadata(null, StylePushBindingsChanged));

        public static PushBindingCollection GetStylePushBindings(DependencyObject depObj)
        {
            if (depObj == null) { throw new ArgumentNullException(nameof(depObj)); }
            return (PushBindingCollection)depObj.GetValue(StylePushBindingsProperty);
        }
        public static void SetStylePushBindings(DependencyObject depObj, PushBindingCollection value)
        {
            if (depObj == null) { throw new ArgumentNullException(nameof(depObj)); }
            depObj.SetValue(StylePushBindingsProperty, value);
        }

        public static void StylePushBindingsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (target != null && e.NewValue is PushBindingCollection stylePushBindings)
            {
                var pushBindingCollection = GetPushBindings(target);
                foreach (var pushBinding in stylePushBindings)
                {
                    var pushBindingClone = pushBinding.Clone() as PushBinding;
                    pushBindingCollection.Add(pushBindingClone!);
                }
            }
        }
    }
}
