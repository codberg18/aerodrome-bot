using System.Windows.Media;

namespace AerodromeSniper.Models;

public class ActivityRow
{
    public required string Kind { get; init; }
    public required string Text { get; init; }
    public required string TimeText { get; init; }
    public required SolidColorBrush KindBrush { get; init; }
}
