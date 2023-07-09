using System;
using System.Windows;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Converts boolean values to string while allowing to configure true and false values.
    /// </summary>
    public sealed class BooleanToStringConverter : BooleanConverter<string>
    {
        public BooleanToStringConverter() :
            base("True", "False")
        { }
    }
}
