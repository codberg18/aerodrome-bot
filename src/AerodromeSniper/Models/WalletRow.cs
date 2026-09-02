using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AerodromeSniper.Models;

public partial class WalletRow : ObservableObject
{
    public required string Address { get; init; }
    public required string Label { get; init; }
    public required string Initials { get; init; }
    public required SolidColorBrush Accent { get; init; }
    public required string CopyModeText { get; set; }
    public required string StatsText { get; init; }
    public required string PnlText { get; init; }
    public double PnlPct { get; init; }

    [ObservableProperty]
    private bool _enabled = true;
}
