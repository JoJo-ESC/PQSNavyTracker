using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace PqsTracker.Desktop.Converters;

// Binds a nullable RevokedAt timestamp directly to a TextBlock's
// TextDecorations — non-null (revoked) gets struck through, null (active)
// gets no decoration. Same pattern WPF uses (IValueConverter), just
// Avalonia's own type underneath.
public class RevokedToStrikethroughConverter : IValueConverter
{
    public static readonly RevokedToStrikethroughConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is DateTime ? TextDecorations.Strikethrough : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
