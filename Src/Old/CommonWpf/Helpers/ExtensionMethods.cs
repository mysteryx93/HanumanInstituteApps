using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HanumanInstitute.CommonServices;

namespace HanumanInstitute.CommonWpf
{
    public static class WpfExtensionMethods
    {
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject d) where T : DependencyObject
        {
            d.CheckNotNull(nameof(d));

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);
                if (child != null)
                {
                    if (child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (var childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        public static bool IsValid(this DependencyObject d)
        {
            // The dependency object is valid if it has no errors and all
            // of its children (that are dependency objects) are error-free.
            return !Validation.GetHasError(d) &&
                LogicalTreeHelper.GetChildren(d)
                .OfType<DependencyObject>()
                .All(IsValid);
        }
    }
}
