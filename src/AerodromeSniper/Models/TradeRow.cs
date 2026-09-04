using System.Windows.Media;

namespace AerodromeSniper.Models;

public class TradeRow
{
    public required string TimeText { get; init; }
    public required string Side { get; init; }
    public required SolidColorBrush SideBrush { get; init; }
    public required string Source { get; init; }
    public required string Symbol { get; init; }
    public required string QuantityText { get; init; }
    public required string PriceText { get; init; }
    public required string ValueText { get; init; }
    public string RealizedPnlText { get; init; } = "—";
    public double RealizedPnlPct { get; init; }
}
