using AerodromeSniper.Models;
using AerodromeSniper.Services.Storage;
using Microsoft.Extensions.Logging;

namespace AerodromeSniper.Services.Trading;

public class TradingEngine : ITradingEngine
{
    private const string FileName = "account.json";

    private readonly IJsonStore _store;
    private readonly ITradeJournal _journal;
    private readonly ISettingsStore _settings;
    private readonly ILogger<TradingEngine> _logger;
    private readonly object _sync = new();

    public AccountState State { get; } = new();

    public event Action? AccountChanged;

    public TradingEngine(IJsonStore store, ITradeJournal journal, ISettingsStore settings,
        ILogger<TradingEngine> logger)
    {
        _store = store;
        _journal = journal;
        _settings = settings;
        _logger = logger;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        var loaded = await _store.LoadAsync<AccountState>(FileName, ct);
        if (loaded is not null)
        {
            State.QuoteUsd = loaded.QuoteUsd;
            State.WethBalance = loaded.WethBalance;
            State.Positions = loaded.Positions;
        }
        _logger.LogInformation("Account loaded: {Quote} USDC, {Weth} WETH, {Positions} positions",
            State.QuoteUsd, State.WethBalance, State.Positions.Count);
    }

    public async Task<Trade> BuyAsync(TokenRef token, decimal quoteUsd, decimal priceUsd, TradeOrigin origin,
        string poolAddress, CancellationToken ct = default)
    {
        // TODO: persist on a timer instead of after every fill,
        // the json rewrite shows up in profilers on busy sessions
        Trade trade;
        lock (_sync)
        {
            if (quoteUsd <= 0 || priceUsd <= 0) throw new ArgumentOutOfRangeException(nameof(quoteUsd));
            if (quoteUsd > State.QuoteUsd) throw new InvalidOperationException("Insufficient USDC balance");

            var fee = Math.Round(quoteUsd * _settings.Current.FeePct / 100m, 6);
            // HACK: 18 decimals because decimal division drift otherwise
            // surfaces in the ui as 0.0000000000000001 on tiny prices
            var fillPrice = Math.Round(priceUsd * (1 + _settings.Current.SlippagePct / 100m), 18);
            var quantity = (quoteUsd - fee) / fillPrice;

            State.QuoteUsd -= quoteUsd;
            var position = State.Positions.FirstOrDefault(p => p.TokenAddress == token.Address);
            if (position is null)
            {
                State.Positions.Add(new Position
                {
                    TokenAddress = token.Address,
                    Symbol = token.Symbol,
                    Quantity = quantity,
                    AvgEntryUsd = fillPrice
                });
            }
            else
            {
                position.AvgEntryUsd = (position.AvgEntryUsd * position.Quantity + fillPrice * quantity) /
                                       (position.Quantity + quantity);
                position.Quantity += quantity;
            }

            trade = new Trade
            {
                Side = TradeSide.Buy,
                Origin = origin,
                PoolAddress = poolAddress,
                TokenAddress = token.Address,
                Symbol = token.Symbol,
                Quantity = quantity,
                PriceUsd = fillPrice,
                QuoteAmountUsd = quoteUsd,
                FeeUsd = fee
            };
            _logger.LogInformation("BUY {Symbol} {Quantity} @ {Price} ({Origin}), fee {Fee}",
                token.Symbol, quantity, fillPrice, origin, fee);
        }

        await _journal.RecordAsync(trade, ct);
        await PersistAsync(ct);
        AccountChanged?.Invoke();
        return trade;
    }

    public async Task<Trade> SellAsync(TokenRef token, decimal quantity, decimal priceUsd, TradeOrigin origin,
        string poolAddress, CancellationToken ct = default)
    {
        Trade trade;
        lock (_sync)
        {
            var position = State.Positions.FirstOrDefault(p => p.TokenAddress == token.Address)
                             ?? throw new InvalidOperationException($"No position in {token.Symbol}");
            if (quantity <= 0 || quantity > position.Quantity)
                throw new ArgumentOutOfRangeException(nameof(quantity));

            var fillPrice = Math.Round(priceUsd * (1 - _settings.Current.SlippagePct / 100m), 18);
            var gross = quantity * fillPrice;
            var fee = Math.Round(gross * _settings.Current.FeePct / 100m, 6);
            var realized = gross - fee - quantity * position.AvgEntryUsd;

            State.QuoteUsd += gross - fee;
            position.Quantity -= quantity;
            if (position.Quantity == 0) State.Positions.Remove(position);

            trade = new Trade
            {
                Side = TradeSide.Sell,
                Origin = origin,
                PoolAddress = poolAddress,
                TokenAddress = token.Address,
                Symbol = token.Symbol,
                Quantity = quantity,
                PriceUsd = fillPrice,
                QuoteAmountUsd = gross,
                FeeUsd = fee,
                RealizedPnlUsd = Math.Round(realized, 6)
            };
            _logger.LogInformation("SELL {Symbol} {Quantity} @ {Price} ({Origin}), realized {Pnl}",
                token.Symbol, quantity, fillPrice, origin, trade.RealizedPnlUsd);
        }

        await _journal.RecordAsync(trade, ct);
        await PersistAsync(ct);
        AccountChanged?.Invoke();
        return trade;
    }

    private Task PersistAsync(CancellationToken ct) => _store.SaveAsync(FileName, State, ct);
}
