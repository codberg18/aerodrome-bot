using CommunityToolkit.Mvvm.ComponentModel;

namespace AerodromeSniper.Models;

public partial class SniperRuleRow : ObservableObject
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Name { get; init; }

    public required string FiltersText { get; init; }

    public required string BuyAmountText { get; init; }

    public string TpSlText { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _enabled = true;
}
