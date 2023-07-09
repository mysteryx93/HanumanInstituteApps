using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Used by PushBinding.
    /// </summary>
    public class FreezableBinding : Freezable
    {
        private Binding? _binding;
        protected Binding Binding => _binding ?? (_binding = new Binding());

        [DefaultValue(null)]
        public object AsyncState
        {
            get => Binding.AsyncState;
            set => Binding.AsyncState = value;
        }

        [DefaultValue(false)]
        public bool BindsDirectlyToSource
        {
            get => Binding.BindsDirectlyToSource;
            set => Binding.BindsDirectlyToSource = value;
        }

        [DefaultValue(null)]
        public IValueConverter Converter
        {
            get => Binding.Converter;
            set => Binding.Converter = value;
        }

        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter)), DefaultValue(null)]
        public CultureInfo ConverterCulture
        {
            get => Binding.ConverterCulture;
            set => Binding.ConverterCulture = value;
        }

        [DefaultValue(null)]

        public object ConverterParameter
        {
            get => Binding.ConverterParameter;
            set => Binding.ConverterParameter = value;
        }

        [DefaultValue(null)]
        public string ElementName
        {
            get => Binding.ElementName;
            set => Binding.ElementName = value;
        }

        [DefaultValue(null)]
        public object FallbackValue
        {
            get => Binding.FallbackValue;
            set => Binding.FallbackValue = value;
        }

        [DefaultValue(false)]
        public bool IsAsync
        {
            get => Binding.IsAsync;
            set => Binding.IsAsync = value;
        }

        [DefaultValue(BindingMode.Default)]
        public BindingMode Mode
        {
            get => Binding.Mode;
            set => Binding.Mode = value;
        }

        [DefaultValue(false)]
        public bool NotifyOnSourceUpdated
        {
            get => Binding.NotifyOnSourceUpdated;
            set => Binding.NotifyOnSourceUpdated = value;
        }

        [DefaultValue(false)]
        public bool NotifyOnTargetUpdated
        {
            get => Binding.NotifyOnTargetUpdated;
            set => Binding.NotifyOnTargetUpdated = value;
        }

        [DefaultValue(false)]
        public bool NotifyOnValidationError
        {
            get => Binding.NotifyOnValidationError;
            set => Binding.NotifyOnValidationError = value;
        }

        [DefaultValue(null)]
        public PropertyPath Path
        {
            get => Binding.Path;
            set => Binding.Path = value;
        }

        [DefaultValue(null)]
        public RelativeSource RelativeSource
        {
            get => Binding.RelativeSource;
            set => Binding.RelativeSource = value;
        }

        [DefaultValue(null)]
        public object Source
        {
            get => Binding.Source;
            set => Binding.Source = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public UpdateSourceExceptionFilterCallback UpdateSourceExceptionFilter
        {
            get => Binding.UpdateSourceExceptionFilter;
            set => Binding.UpdateSourceExceptionFilter = value;
        }

        [DefaultValue(UpdateSourceTrigger.PropertyChanged)]
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get => Binding.UpdateSourceTrigger;
            set => Binding.UpdateSourceTrigger = value;
        }

        [DefaultValue(false)]
        public bool ValidatesOnDataErrors
        {
            get => Binding.ValidatesOnDataErrors;
            set => Binding.ValidatesOnDataErrors = value;
        }

        [DefaultValue(false)]
        public bool ValidatesOnExceptions
        {
            get => Binding.ValidatesOnExceptions;
            set => Binding.ValidatesOnExceptions = value;
        }

        [DefaultValue(null)]
        public string XPath
        {
            get => Binding.XPath;
            set => Binding.XPath = value;
        }

        [DefaultValue(null)]
        public Collection<ValidationRule> ValidationRules => Binding.ValidationRules;



        protected override void CloneCore(Freezable sourceFreezable)
        {
            if (!(sourceFreezable is FreezableBinding clone)) { throw new ArgumentNullException(nameof(sourceFreezable)); }

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
            foreach (var validationRule in clone.ValidationRules)
            {
                ValidationRules.Add(validationRule);
            }
            base.CloneCore(sourceFreezable);
        }

        protected override Freezable CreateInstanceCore() => new FreezableBinding();
    }
}
