using System.Windows.Media;

namespace AerodromeSniper.Models;

public class PoolRow
{
    public required string Address { get; init; }
    public required string BaseSymbol { get; init; }
    public required string QuoteSymbol { get; init; }
    public required string Initials { get; init; }
    public required SolidColorBrush Accent { get; init; }
    public required string DexTag { get; init; }
    public double AgeMinutes { get; set; }
    public required string AgeText { get; set; }
    public double LiquidityUsd { get; init; }
    public required string LiquidityText { get; init; }
    public required string VolumeText { get; init; }
    public required string PriceText { get; init; }
    public double ChangePct { get; init; }
    public bool IsNew { get; set; }
}
