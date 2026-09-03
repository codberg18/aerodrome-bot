using System.Windows.Media;

namespace AerodromeSniper.Models;

public class CopyFeedRow
{
    public required string TimeText { get; init; }
    public required string WalletLabel { get; init; }
    public required string TheirText { get; init; }
    public required string OurText { get; init; }
    public required string StatusText { get; init; }
    public required SolidColorBrush StatusBrush { get; init; }
    public required SolidColorBrush StatusBackground { get; init; }
}
