using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

// OneWayToSource Binding for ReadOnly Dependency Property
// https://meleak.wordpress.com/2011/08/28/onewaytosource-binding-for-readonly-dependency-property/
// License: public domain

namespace HanumanInstitute.CommonWpf
{

    public class FreezableBinding : Freezable
    {
        #region Properties

        private Binding _binding;
        protected Binding Binding {
            get {
                if (_binding == null)
                {
                    _binding = new Binding();
                }
                return _binding;
            }
        }

        [DefaultValue(null)]
        public object AsyncState {
            get => Binding.AsyncState;
            set => Binding.AsyncState = value;
        }

        [DefaultValue(false)]
        public bool BindsDirectlyToSource {
            get => Binding.BindsDirectlyToSource;
            set => Binding.BindsDirectlyToSource = value;
        }

        [DefaultValue(null)]
        public IValueConverter Converter {
            get => Binding.Converter;
            set => Binding.Converter = value;
        }

        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter)), DefaultValue(null)]
        public CultureInfo ConverterCulture {
            get => Binding.ConverterCulture;
            set => Binding.ConverterCulture = value;
        }

        [DefaultValue(null)]

        public object ConverterParameter {
            get => Binding.ConverterParameter;
            set => Binding.ConverterParameter = value;
        }

        [DefaultValue(null)]
        public string ElementName {
            get => Binding.ElementName;
            set => Binding.ElementName = value;
        }

        [DefaultValue(null)]
        public object FallbackValue {
            get => Binding.FallbackValue;
            set => Binding.FallbackValue = value;
        }

        [DefaultValue(false)]
        public bool IsAsync {
            get => Binding.IsAsync;
            set => Binding.IsAsync = value;
        }

        [DefaultValue(BindingMode.Default)]
        public BindingMode Mode {
            get => Binding.Mode;
            set => Binding.Mode = value;
        }

        [DefaultValue(false)]
        public bool NotifyOnSourceUpdated {
            get => Binding.NotifyOnSourceUpdated;
            set => Binding.NotifyOnSourceUpdated = value;
        }

        [DefaultValue(false)]
        public bool NotifyOnTargetUpdated {
            get => Binding.NotifyOnTargetUpdated;
            set => Binding.NotifyOnTargetUpdated = value;
        }

        [DefaultValue(false)]
        public bool NotifyOnValidationError {
            get => Binding.NotifyOnValidationError;
            set => Binding.NotifyOnValidationError = value;
        }

        [DefaultValue(null)]
        public PropertyPath Path {
            get => Binding.Path;
            set => Binding.Path = value;
        }

        [DefaultValue(null)]
        public RelativeSource RelativeSource {
            get => Binding.RelativeSource;
            set => Binding.RelativeSource = value;
        }

        [DefaultValue(null)]
        public object Source {
            get => Binding.Source;
            set => Binding.Source = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter {
            get => Binding.UpdateSourceExceptionFilter;
            set => Binding.UpdateSourceExceptionFilter = value;
        }

        [DefaultValue(UpdateSourceTrigger.PropertyChanged)]
        public UpdateSourceTrigger UpdateSourceTrigger {
            get => Binding.UpdateSourceTrigger;
            set => Binding.UpdateSourceTrigger = value;
        }

        [DefaultValue(false)]
        public bool ValidatesOnDataErrors {
            get => Binding.ValidatesOnDataErrors;
            set => Binding.ValidatesOnDataErrors = value;
        }

        [DefaultValue(false)]
        public bool ValidatesOnExceptions {
            get => Binding.ValidatesOnExceptions;
            set => Binding.ValidatesOnExceptions = value;
        }

        [DefaultValue(null)]
        public string XPath {
            get => Binding.XPath;
            set => Binding.XPath = value;
        }

        [DefaultValue(null)]
        public Collection<ValidationRule> ValidationRules => Binding.ValidationRules;

        #endregion // Properties

        #region Freezable overrides

        protected override void CloneCore(Freezable sourceFreezable)
        {
            if (sourceFreezable == null) { throw new ArgumentNullException(nameof(sourceFreezable)); }

            FreezableBinding clone = sourceFreezable as FreezableBinding;
            if (clone.ElementName != null)
            {
                ElementName = clone.ElementName;
            }
            else if (clone.RelativeSource != null)
            {
                RelativeSource = clone.RelativeSource;
            }
            else if (clone.Source != null)
            {
                Source = clone.Source;
            }
            AsyncState = clone.AsyncState;
            BindsDirectlyToSource = clone.BindsDirectlyToSource;
            Converter = clone.Converter;
            ConverterCulture = clone.ConverterCulture;
            ConverterParameter = clone.ConverterParameter;
            FallbackValue = clone.FallbackValue;
            IsAsync = clone.IsAsync;
            Mode = clone.Mode;
            NotifyOnSourceUpdated = clone.NotifyOnSourceUpdated;
            NotifyOnTargetUpdated = clone.NotifyOnTargetUpdated;
            NotifyOnValidationError = clone.NotifyOnValidationError;
            Path = clone.Path;
            UpdateSourceExceptionFilter = clone.UpdateSourceExceptionFilter;
            UpdateSourceTrigger = clone.UpdateSourceTrigger;
            ValidatesOnDataErrors = clone.ValidatesOnDataErrors;
            ValidatesOnExceptions = clone.ValidatesOnExceptions;
            XPath = clone.XPath;
            foreach (ValidationRule validationRule in clone.ValidationRules)
            {
                ValidationRules.Add(validationRule);
            }
            base.CloneCore(sourceFreezable);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new FreezableBinding();
        }

        #endregion // Freezable overrides
    }



    public class PushBinding : FreezableBinding
    {
        #region Dependency Properties

        public static readonly DependencyProperty TargetPropertyMirrorProperty =
            DependencyProperty.Register("TargetPropertyMirror",
                                        typeof(object),
                                        typeof(PushBinding));
        public static readonly DependencyProperty TargetPropertyListenerProperty =
            DependencyProperty.Register("TargetPropertyListener",
                                        typeof(object),
                                        typeof(PushBinding),
                                        new UIPropertyMetadata(null, OnTargetPropertyListenerChanged));

        private static void OnTargetPropertyListenerChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PushBinding pushBinding = sender as PushBinding;
            pushBinding.TargetPropertyValueChanged();
        }

        #endregion // Dependency Properties

        #region Constructor

        public PushBinding()
        {
            Mode = BindingMode.OneWayToSource;
        }

        #endregion // Constructor

        #region Properties

        public object TargetPropertyMirror {
            get => GetValue(TargetPropertyMirrorProperty);
            set => SetValue(TargetPropertyMirrorProperty, value);
        }
        public object TargetPropertyListener {
            get => GetValue(TargetPropertyListenerProperty);
            set => SetValue(TargetPropertyListenerProperty, value);
        }

        [DefaultValue(null)]
        public string TargetProperty {
            get;
            set;
        }

        [DefaultValue(null)]
        public DependencyProperty TargetDependencyProperty {
            get;
            set;
        }

        #endregion // Properties

        #region Public Methods

        public void SetupTargetBinding(DependencyObject targetObject)
        {
            if (targetObject == null)
            {
                return;
            }

            // Prevent the designer from reporting exceptions since
            // changes will be made of a Binding in use if it is set
            if (DesignerProperties.GetIsInDesignMode(this) == true)
            {
                return;
            }

            // Bind to the selected TargetProperty, e.g. ActualHeight and get
            // notified about changes in OnTargetPropertyListenerChanged
            Binding listenerBinding = new Binding
            {
                Source = targetObject,
                Mode = BindingMode.OneWay
            };
            if (TargetDependencyProperty != null)
            {
                listenerBinding.Path = new PropertyPath(TargetDependencyProperty);
            }
            else
            {
                listenerBinding.Path = new PropertyPath(TargetProperty);
            }
            BindingOperations.SetBinding(this, TargetPropertyListenerProperty, listenerBinding);

            // Set up a OneWayToSource Binding with the Binding declared in Xaml from
            // the Mirror property of this class. The mirror property will be updated
            // everytime the Listener property gets updated
            BindingOperations.SetBinding(this, TargetPropertyMirrorProperty, Binding);

            TargetPropertyValueChanged();
            if (targetObject is FrameworkElement)
            {
                ((FrameworkElement)targetObject).Loaded += delegate { TargetPropertyValueChanged(); };
            }
            else if (targetObject is FrameworkContentElement)
            {
                ((FrameworkContentElement)targetObject).Loaded += delegate { TargetPropertyValueChanged(); };
            }
        }

        #endregion // Public Methods

        #region Private Methods

        private void TargetPropertyValueChanged()
        {
            object targetPropertyValue = GetValue(TargetPropertyListenerProperty);
            SetValue(TargetPropertyMirrorProperty, targetPropertyValue);
        }

        #endregion // Private Methods

        #region Freezable overrides

        protected override void CloneCore(Freezable sourceFreezable)
        {
            if (sourceFreezable == null) { throw new ArgumentNullException(nameof(sourceFreezable)); }

            PushBinding pushBinding = sourceFreezable as PushBinding;
            TargetProperty = pushBinding.TargetProperty;
            TargetDependencyProperty = pushBinding.TargetDependencyProperty;
            base.CloneCore(sourceFreezable);
        }

        protected override Freezable CreateInstanceCore()
        {
            return new PushBinding();
        }

        #endregion // Freezable overrides
    }



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
            if (target != null)
            {
                PushBindingCollection stylePushBindings = e.NewValue as PushBindingCollection;
                PushBindingCollection pushBindingCollection = GetPushBindings(target);
                foreach (PushBinding pushBinding in stylePushBindings)
                {
                    PushBinding pushBindingClone = pushBinding.Clone() as PushBinding;
                    pushBindingCollection.Add(pushBindingClone);
                }
            }
        }
    }




    public class PushBindingCollection : FreezableCollection<PushBinding>
    {
        public PushBindingCollection() { }

        public PushBindingCollection(DependencyObject targetObject)
        {
            TargetObject = targetObject;
            ((INotifyCollectionChanged)this).CollectionChanged += CollectionChanged;
        }

        void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PushBinding pushBinding in e.NewItems)
                {
                    pushBinding.SetupTargetBinding(TargetObject);
                }
            }
        }

        public DependencyObject TargetObject {
            get;
            private set;
        }
    }
}
