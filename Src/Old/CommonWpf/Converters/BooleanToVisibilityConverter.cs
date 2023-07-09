using System;
using System.Windows;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Converts boolean values to Visibility while allowing to configure true and false values.
    /// </summary>
    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Collapsed)
        { }
    }
}
