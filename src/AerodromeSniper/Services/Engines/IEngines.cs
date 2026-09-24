using AerodromeSniper.Models;

namespace AerodromeSniper.Services.Engines;

public interface ISniperEngine
{
    bool Running { get; }
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}

public interface ICopyEngine
{
    bool Running { get; }
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}

public record SniperRule
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; init; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public decimal MinLiquidityUsd { get; init; }
    public int MaxAgeMinutes { get; init; }
    public string? QuoteSymbol { get; init; }
    public decimal BuyAmountUsd { get; init; }
    public decimal? TakeProfitPct { get; init; }
    public decimal? StopLossPct { get; init; }
}

public record WatchedWallet
{
    public string Address { get; init; } = string.Empty;
    public string Label { get; init; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public bool PercentMode { get; init; }
    public decimal CopyAmount { get; init; }
}
