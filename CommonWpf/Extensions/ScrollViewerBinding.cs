using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using LinqToVisualTree;

// Based on
// https://blog.scottlogic.com/2010/07/21/exposing-and-binding-to-a-silverlight-scrollviewers-scrollbars.html
// Edited to coerce dependency property values and restructured code.

namespace HanumanInstitute.CommonWpf {
    /// <summary>
    /// Attached behaviour which exposes the horizontal and vertical offset values
    /// for a ScrollViewer, permitting binding.
    /// NOTE: This code could be simplified a little by finding common code between vertical / horizontal
    /// scrollbars. However, this was not doen for clarity in the associated blog post!
    /// </summary>
    public static class ScrollViewerBinding {

        #region Properties

        // VerticalOffset
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBinding),
            new PropertyMetadata(-1.0, VerticalOffsetChanged));
        public static double GetVerticalOffset(DependencyObject depObj) => (double)depObj.GetValue(VerticalOffsetProperty);
        public static void SetVerticalOffset(DependencyObject depObj, double value) => depObj.SetValue(VerticalOffsetProperty, value);

        // HorizontalOffset
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ScrollViewerBinding),
            new PropertyMetadata(-1.0, HorizontalOffsetChanged));
        public static double GetHorizontalOffset(DependencyObject depObj) => (double)depObj.GetValue(HorizontalOffsetProperty);
        public static void SetHorizontalOffset(DependencyObject depObj, double value) => depObj.SetValue(HorizontalOffsetProperty, value);

        /// <summary>
        /// An attached property which holds a reference to the vertical scrollbar which
        /// is extracted from the visual tree of a ScrollViewer
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarProperty = DependencyProperty.RegisterAttached("VerticalScrollBar", typeof(ScrollBar),
            typeof(ScrollViewerBinding), new PropertyMetadata(null));
        public static ScrollBar GetVerticalScrollBar(DependencyObject depObj) => (ScrollBar)depObj?.GetValue(VerticalScrollBarProperty);
        public static void SetVerticalScrollBar(DependencyObject depObj, ScrollBar value) => depObj.SetValue(VerticalScrollBarProperty, value);

        /// <summary>
        /// An attached property which holds a reference to the horizontal scrollbar which
        /// is extracted from the visual tree of a ScrollViewer
        /// </summary>
        public static readonly DependencyProperty HorizontalScrollBarProperty = DependencyProperty.RegisterAttached("HorizontalScrollBar", typeof(ScrollBar),
            typeof(ScrollViewerBinding), new PropertyMetadata(null));
        public static ScrollBar GetHorizontalScrollBar(DependencyObject depObj) => (ScrollBar)depObj?.GetValue(HorizontalScrollBarProperty);
        public static void SetHorizontalScrollBar(DependencyObject depObj, ScrollBar value) => depObj.SetValue(HorizontalScrollBarProperty, value);

        #endregion


        /// <summary>
        /// Invoked when the VerticalOffset attached property changes
        /// </summary>
        private static void VerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is ScrollViewer sv) {
                // check whether we have a reference to the vertical scrollbar
                if (GetVerticalScrollBar(sv) == null) {
                    // if not, handle LayoutUpdated, which will be invoked after the
                    // template is applied and extract the scrollbar
                    sv.LayoutUpdated += (s, ev) => {
                        if (GetVerticalScrollBar(sv) == null)
                            GetScrollBarsForScrollViewer(sv);
                    };
                } else {
                    // update the scrollviewer offset
                    sv.ScrollToVerticalOffset((double)e.NewValue);
                }
            }
        }

        /// <summary>
        /// Invoked when the HorizontalOffset attached property changes
        /// </summary>
        private static void HorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is ScrollViewer sv) {
                // check whether we have a reference to the vertical scrollbar
                if (GetHorizontalScrollBar(sv) == null) {
                    // if not, handle LayoutUpdated, which will be invoked after the
                    // template is applied and extract the scrollbar
                    sv.LayoutUpdated += (s, ev) => {
                        if (GetHorizontalScrollBar(sv) == null)
                            GetScrollBarsForScrollViewer(sv);
                    };
                } else {
                    // update the scrollviewer offset
                    sv.ScrollToHorizontalOffset((double)e.NewValue);
                }
            }
        }

        /// <summary>
        /// Attempts to extract the scrollbars that are within the scrollviewers
        /// visual tree. When extracted, event handlers are added to their ValueChanged events.
        /// </summary>
        private static void GetScrollBarsForScrollViewer(ScrollViewer scrollViewer) {
            ScrollBar scroll = GetScrollBar(scrollViewer, Orientation.Vertical);
            if (scroll != null) {
                // save a reference to this scrollbar on the attached property
                SetVerticalScrollBar(scrollViewer, scroll);

                // scroll the scrollviewer
                scrollViewer.ScrollToVerticalOffset(ScrollViewerBinding.GetVerticalOffset(scrollViewer));

                // handle the changed event to update the exposed VerticalOffset
                scroll.ValueChanged += (s, e) => SetVerticalOffset(scrollViewer, e.NewValue);
            }

            scroll = GetScrollBar(scrollViewer, Orientation.Horizontal);
            if (scroll != null) {
                // save a reference to this scrollbar on the attached property
                SetHorizontalScrollBar(scrollViewer, scroll);

                // scroll the scrollviewer
                scrollViewer.ScrollToHorizontalOffset(ScrollViewerBinding.GetHorizontalOffset(scrollViewer));

                // handle the changed event to update the exposed HorizontalOffset
                scroll.ValueChanged += (s, e) => SetHorizontalOffset(scrollViewer, e.NewValue);
            }
        }

        /// <summary>
        /// Searches the descendants of the given element, looking for a scrollbar
        /// with the given orientation.
        /// </summary>
        private static ScrollBar GetScrollBar(FrameworkElement fe, Orientation orientation) {
            return fe.Descendants()
                      .OfType<ScrollBar>()
                      .Where(s => s.Orientation == orientation)
                      .SingleOrDefault();
        }

        /// <summary>
        /// Scroll values are not auutomatically coerced. Call this method to manually coerce values.
        /// </summary>
        /// <param name="d">The ScrollViewer containing the attached property.</param>
        public static void CoerceOffset(DependencyObject d) {
            double Original = GetVerticalOffset(d);
            double Value = Original;
            ScrollBar Scroll = GetVerticalScrollBar(d);
            if (Scroll != null) {
                Value = Math.Max(Value, Scroll.Minimum);
                Value = Math.Min(Value, Scroll.Maximum);
                if (Value != Original)
                    SetVerticalOffset(d, Value);
            }

            Original = GetHorizontalOffset(d);
            Value = Original;
            Scroll = GetHorizontalScrollBar(d);
            if (Scroll != null) {
                Value = Math.Max(Value, Scroll.Minimum);
                Value = Math.Min(Value, Scroll.Maximum);
                if (Value != Original)
                    SetHorizontalOffset(d, Value);
            }
        }
    }
}
