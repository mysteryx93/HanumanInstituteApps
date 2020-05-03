using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using HanumanInstitute.CommonServices;

// OneWayToSource Binding for ReadOnly Dependency Properties
// https://meleak.wordpress.com/2011/08/28/onewaytosource-binding-for-readonly-dependency-property/
// License: public domain
namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// OneWayToSource Binding for ReadOnly Dependency Properties.
    /// </summary>
    public class PushBinding : FreezableBinding
    {
        // TargetPropertyMirror
        public static readonly DependencyProperty TargetPropertyMirrorProperty = DependencyProperty.Register("TargetPropertyMirror", typeof(object), typeof(PushBinding));
        public object TargetPropertyMirror { get => GetValue(TargetPropertyMirrorProperty); set => SetValue(TargetPropertyMirrorProperty, value); }

        // TargetPropertyListener
        public static readonly DependencyProperty TargetPropertyListenerProperty = DependencyProperty.Register("TargetPropertyListener", typeof(object), typeof(PushBinding),
            new UIPropertyMetadata(null, OnTargetPropertyListenerChanged));
        public object TargetPropertyListener { get => GetValue(TargetPropertyListenerProperty); set => SetValue(TargetPropertyListenerProperty, value); }
        private static void OnTargetPropertyListenerChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as PushBinding)?.TargetPropertyValueChanged();
        }


        public PushBinding()
        {
            Mode = BindingMode.OneWayToSource;
        }

        [DefaultValue(null)]
        public string? TargetProperty { get; set; }

        [DefaultValue(null)]
        public DependencyProperty? TargetDependencyProperty { get; set; }

        public void SetupTargetBinding(DependencyObject? targetObject)
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
            var listenerBinding = new Binding
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

        private void TargetPropertyValueChanged()
        {
            var targetPropertyValue = GetValue(TargetPropertyListenerProperty);
            SetValue(TargetPropertyMirrorProperty, targetPropertyValue);
        }

        protected override void CloneCore(Freezable sourceFreezable)
        {
            sourceFreezable.CheckNotNull(nameof(sourceFreezable));

            if (sourceFreezable is PushBinding pushBinding)
            {
                TargetProperty = pushBinding.TargetProperty;
                TargetDependencyProperty = pushBinding.TargetDependencyProperty;
            }
            base.CloneCore(sourceFreezable);
        }

        protected override Freezable CreateInstanceCore() => new PushBinding();
    }
}
