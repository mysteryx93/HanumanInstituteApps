// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace HanumanInstitute.Avalonia;

/// <summary>
/// Represents an item to display in a ComboBox.
/// </summary>
public class ListItem<T>
{
    public string Text { get; set; }
    public T Value { get; set; }

    public ListItem(string text, T value)
    {
        Text = text;
        Value = value;
    }

    public override string ToString() => Text;
}
