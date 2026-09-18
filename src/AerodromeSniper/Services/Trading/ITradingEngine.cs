using AerodromeSniper.Models;

namespace AerodromeSniper.Services.Trading;

public interface ITradeJournal
{
    IReadOnlyList<Trade> All { get; }
    Task LoadAsync(CancellationToken ct = default);
    Task RecordAsync(Trade trade, CancellationToken ct = default);
}

public interface ITradingEngine
{
    AccountState State { get; }
    event Action? AccountChanged;
    Task LoadAsync(CancellationToken ct = default);
    Task<Trade> BuyAsync(TokenRef token, decimal quoteUsd, decimal priceUsd, TradeOrigin origin,
        string poolAddress, CancellationToken ct = default);
    Task<Trade> SellAsync(TokenRef token, decimal quantity, decimal priceUsd, TradeOrigin origin,
        string poolAddress, CancellationToken ct = default);
}
