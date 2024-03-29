﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HanumanInstitute.Services.Properties {
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("HanumanInstitute.Services.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to API method {0} failed and returned HRESULT {1}.
        /// </summary>
        internal static string ApiInvocationError {
            get {
                return ResourceManager.GetString("ApiInvocationError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} cannot be null or empty..
        /// </summary>
        internal static string ArgumentNullOrEmpty {
            get {
                return ResourceManager.GetString("ArgumentNullOrEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BooleanAndConverter is a OneWay converter..
        /// </summary>
        internal static string BooleanAndConverterConvertBackNotSupported {
            get {
                return ResourceManager.GetString("BooleanAndConverterConvertBackNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot delete file &quot;{0}&quot;..
        /// </summary>
        internal static string CannotDeleteFile {
            get {
                return ResourceManager.GetString("CannotDeleteFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Current property is null..
        /// </summary>
        internal static string GenericSettingsProviderCurrentNull {
            get {
                return ResourceManager.GetString("GenericSettingsProviderCurrentNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot save settings because of validation errors..
        /// </summary>
        internal static string GenericSettingsProviderValidationErrors {
            get {
                return ResourceManager.GetString("GenericSettingsProviderValidationErrors", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The target property must be a boolean..
        /// </summary>
        internal static string InverseBooleanConverterTargetTypeNotBoolean {
            get {
                return ResourceManager.GetString("InverseBooleanConverterTargetTypeNotBoolean", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value is invalid.
        /// </summary>
        internal static string NumericRangeRuleInvalid {
            get {
                return ResourceManager.GetString("NumericRangeRuleInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value must be a multiple of {0}.
        /// </summary>
        internal static string NumericRangeRuleNotMultiple {
            get {
                return ResourceManager.GetString("NumericRangeRuleNotMultiple", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Value cannot be null.
        /// </summary>
        internal static string NumericRangeRuleNull {
            get {
                return ResourceManager.GetString("NumericRangeRuleNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Please enter a value in the range: {0} - {1}.
        /// </summary>
        internal static string NumericRangeRuleOutOfRange {
            get {
                return ResourceManager.GetString("NumericRangeRuleOutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} of type &apos;{1}&apos; must be assignable from type &apos;{2}&apos;..
        /// </summary>
        internal static string TypeMustBeAssignableFromBase {
            get {
                return ResourceManager.GetString("TypeMustBeAssignableFromBase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} of type &apos;{1}&apos; must derive from type &apos;{2}&apos;..
        /// </summary>
        internal static string TypeMustDeriveFromBase {
            get {
                return ResourceManager.GetString("TypeMustDeriveFromBase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Validation for {0} failed!.
        /// </summary>
        internal static string ValidationFailed {
            get {
                return ResourceManager.GetString("ValidationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The passed value may not be empty or whithespace..
        /// </summary>
        internal static string ValueIsNullOrWhiteSpace {
            get {
                return ResourceManager.GetString("ValueIsNullOrWhiteSpace", resourceCulture);
            }
        }
    }
}
