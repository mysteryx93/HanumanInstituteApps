using System;
using System.Windows.Media;

namespace HanumanInstitute.CommonWpf
{
    /// <summary>
    /// Converts boolean values to string while allowing to configure true and false values.
    /// </summary>
    public sealed class BooleanToBrushConverter : BooleanConverter<Brush>
    {
        public BooleanToBrushConverter() :
            base(new SolidColorBrush(Color.FromRgb(0, 0, 255)), new SolidColorBrush(Color.FromRgb(255, 0, 0)))
        { }
    }
}
