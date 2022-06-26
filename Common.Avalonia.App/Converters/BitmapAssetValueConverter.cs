using System.Drawing;
using System.Globalization;
using System.Reflection;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Platform;

namespace HanumanInstitute.Common.Avalonia.App;

/// <summary>
/// <para>
/// Converts a string path to a bitmap asset.
/// </para>
/// <para>
/// The asset must be in the same assembly as the program. If it isn't,
/// specify "avares://{AssemblyNameHere}/" in front of the path to the asset.
/// </para>
/// </summary>
public class BitmapAssetValueConverter : IValueConverter
{
    public static BitmapAssetValueConverter Instance { get; } = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) { return null; }

        if (value is string rawUri && targetType.IsAssignableFrom(typeof(Bitmap)))
        {
            Uri uri;

            // Allow for assembly overrides
            if (rawUri.StartsWith("avares://"))
            {
                uri = new Uri(rawUri);
            }
            else
            {
                var assemblyName = Assembly.GetEntryAssembly()?.GetName().Name;
                uri = new Uri($"avares://{assemblyName}{rawUri}");
            }

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
            var asset = assets.Open(uri);

            return new Bitmap(asset);
        }

        throw new NotSupportedException();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
