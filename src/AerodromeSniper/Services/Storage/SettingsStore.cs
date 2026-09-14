using AerodromeSniper.Models;
using Microsoft.Extensions.Logging;

namespace AerodromeSniper.Services.Storage;

public interface ISettingsStore
{
    AppSettings Current { get; }
    Task LoadAsync(CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}

public class SettingsStore : ISettingsStore
{
    private const string FileName = "settings.json";

    private readonly IJsonStore _store;
    private readonly ILogger<SettingsStore> _logger;

    public AppSettings Current { get; private set; } = new();

    public SettingsStore(IJsonStore store, ILogger<SettingsStore> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task LoadAsync(CancellationToken ct = default)
    {
        Current = await _store.LoadAsync<AppSettings>(FileName, ct) ?? new AppSettings();
        _logger.LogInformation("Settings loaded: pools {Pools}s, prices {Prices}s, wallets {Wallets}s",
            Current.NewPoolsIntervalSec, Current.PricesIntervalSec, Current.WalletsIntervalSec);
    }

    public Task SaveAsync(CancellationToken ct = default) => _store.SaveAsync(FileName, Current, ct);
}
