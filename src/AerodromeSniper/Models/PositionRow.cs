using System.Windows.Media;

namespace AerodromeSniper.Models;

public class PositionRow
{
    public required string Symbol { get; init; }
    public required string Initials { get; init; }
    public required string QuantityText { get; init; }
    public required string ValueUsd { get; init; }
    public double PnlPct { get; init; }
    public required SolidColorBrush Accent { get; init; }
    public string EntryText { get; init; } = string.Empty;
    public string LastPriceText { get; init; } = string.Empty;
}
