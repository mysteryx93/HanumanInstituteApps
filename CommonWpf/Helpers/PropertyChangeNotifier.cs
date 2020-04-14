using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

// PropertyDescriptor AddValueChanged Alternative
// https://agsmith.wordpress.com/2008/04/07/propertydescriptor-addvaluechanged-alternative/
// License: public domain

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Tracks changes to a dependency property while avoiding memory leaks.
    /// </summary>
    public sealed class PropertyChangeNotifier : DependencyObject, IDisposable
    {

        private readonly WeakReference _propertySource;

        public PropertyChangeNotifier(DependencyObject propertySource, string path)
        : this(propertySource, new PropertyPath(path))
        {
        }
        public PropertyChangeNotifier(DependencyObject propertySource, DependencyProperty property)
        : this(propertySource, new PropertyPath(property))
        {
        }
        public PropertyChangeNotifier(DependencyObject propertySource, PropertyPath property)
        {
            if (null == propertySource)
                throw new ArgumentNullException(nameof(propertySource));
            if (null == property)
                throw new ArgumentNullException(nameof(property));
            this._propertySource = new WeakReference(propertySource);
            Binding binding = new Binding();
            binding.Path = property;
            binding.Mode = BindingMode.OneWay;
            binding.Source = propertySource;
            BindingOperations.SetBinding(this, ValueProperty, binding);
        }

        public DependencyObject PropertySource {
            get {
                try
                {
                    // note, it is possible that accessing the target property
                    // will result in an exception so i’ve wrapped this check
                    // in a try catch
                    return this._propertySource.IsAlive
                    ? this._propertySource.Target as DependencyObject
                    : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
        typeof(object), typeof(PropertyChangeNotifier), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PropertyChangeNotifier notifier = (PropertyChangeNotifier)d;
            if (null != notifier.ValueChanged)
                notifier.ValueChanged(notifier.PropertySource, EventArgs.Empty);
        }

        /// <summary>
        /// Returns/sets the value of the property
        /// </summary>
        /// <seealso cref="ValueProperty"/>
        [Description("Returns / sets the value of the property")]
        [Category("Behavior")]
        [Bindable(true)]
        public object Value {
            get {
                return (object)this.GetValue(PropertyChangeNotifier.ValueProperty);
            }
            set {
                this.SetValue(PropertyChangeNotifier.ValueProperty, value);
            }
        }

        public event EventHandler ValueChanged;

        private bool disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    BindingOperations.ClearBinding(this, ValueProperty);
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
