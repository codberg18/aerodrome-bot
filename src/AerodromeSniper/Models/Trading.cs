namespace AerodromeSniper.Models;

public enum TradeSide
{
    Buy,
    Sell
}

public enum TradeOrigin
{
    Manual,
    Sniper,
    Copy
}

public class Trade
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public TradeSide Side { get; init; }
    public TradeOrigin Origin { get; init; }
    public string PoolAddress { get; init; } = string.Empty;
    public string TokenAddress { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public decimal PriceUsd { get; init; }
    public decimal QuoteAmountUsd { get; init; }
    public decimal FeeUsd { get; init; }
    public decimal? RealizedPnlUsd { get; init; }
}

public class Position
{
    public string TokenAddress { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal AvgEntryUsd { get; set; }
    public DateTimeOffset OpenedAt { get; set; } = DateTimeOffset.UtcNow;
}

public class AccountState
{
    public decimal QuoteUsd { get; set; } = 10_000m;
    public decimal WethBalance { get; set; } = 5m;
    public List<Position> Positions { get; set; } = [];
    public decimal StartingQuoteUsd { get; init; } = 10_000m;
    public decimal StartingWeth { get; init; } = 5m;
}
