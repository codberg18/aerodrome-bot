namespace AerodromeSniper.Models;

public record TokenRef
{
    public required string Address { get; init; }
    public required string Symbol { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Decimals { get; init; } = 18;
    public string IconUrl { get; init; } = string.Empty;
}

public record PoolInfo
{
    public required string Address { get; init; }
    public required string DexId { get; init; }
    public required TokenRef BaseToken { get; init; }
    public required TokenRef QuoteToken { get; init; }
    public decimal PriceUsd { get; init; }
    public decimal LiquidityUsd { get; init; }
    public decimal Volume24hUsd { get; init; }
    public decimal FdvUsd { get; init; }
    public decimal MarketCapUsd { get; init; }
    public decimal PriceChangePct24h { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record WalletTransfer
{
    public required string TxHash { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public required string TokenAddress { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public required string From { get; init; }
    public required string To { get; init; }
}
