using AerodromeSniper.Models;
using AerodromeSniper.Services.Storage;
using Microsoft.Extensions.Logging;

namespace AerodromeSniper.Services.Trading;

public class TradeJournal : ITradeJournal
{
    private const string FileName = "trades.json";
    private const int Cap = 5000;

    private readonly IJsonStore _store;
    private readonly ILogger<TradeJournal> _logger;
    private readonly List<Trade> _trades = [];

    public IReadOnlyList<Trade> All => _trades;

    public TradeJournal(IJsonStore store, ILogger<TradeJournal> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        var loaded = await _store.LoadAsync<List<Trade>>(FileName, ct);
        _trades.Clear();
        if (loaded is not null) _trades.AddRange(loaded);
        _logger.LogInformation("Trade journal loaded: {Count} entries", _trades.Count);
    }

    public async Task RecordAsync(Trade trade, CancellationToken ct = default)
    {
        // FIXME: cap trims the tail but the whole list is rewritten per trade
        _trades.Insert(0, trade);
        if (_trades.Count > Cap) _trades.RemoveRange(Cap, _trades.Count - Cap);
        await _store.SaveAsync(FileName, _trades, ct);
    }
}
