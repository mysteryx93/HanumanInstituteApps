using Avalonia.Media;

namespace HanumanInstitute.Avalonia;

/// <summary>
/// Converts boolean values to brush while allowing to configure true and false values.
/// </summary>
public sealed class BooleanToBrushConverter : BooleanConverter<Brush>
{
    public BooleanToBrushConverter() :
        base(new SolidColorBrush(Color.FromRgb(0, 0, 255)), new SolidColorBrush(Color.FromRgb(255, 0, 0)))
    { }
}
