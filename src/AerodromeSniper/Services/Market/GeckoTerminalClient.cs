using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AerodromeSniper.Models;
using AerodromeSniper.Services.Storage;
using Microsoft.Extensions.Logging;

namespace AerodromeSniper.Services.Market;

public class GeckoTerminalClient : IMarketDataService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly HashSet<string> AerodromeDexes =
        ["aerodrome-base", "aerodrome-slipstream", "aerodrome-slipstream-2"];

    private readonly HttpClient _http;
    private readonly ISettingsStore _settings;
    private readonly ILogger<GeckoTerminalClient> _logger;

    public GeckoTerminalClient(HttpClient http, ISettingsStore settings, ILogger<GeckoTerminalClient> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PoolInfo>> GetNewPoolsAsync(CancellationToken ct = default)
    {
        // FIXME: the loop stops at the first page that has anything aerodrome,
        // on quiet nights that burns 3 requests for an empty list
        var pools = new List<PoolInfo>();
        for (var page = 1; page <= 3 && pools.Count == 0; page++)
        {
            var url = _settings.Current.GeckoBaseUrl.TrimEnd('/') + $"/networks/base/new_pools?page={page}";
            var dto = await GetAsync<NewPoolsResponse>(url, ct);
            if (dto?.Data is null) continue;

            foreach (var pool in dto.Data)
            {
                var dexId = pool.Relationships?.Dex?.Data?.Id;
                if (dexId is null || !AerodromeDexes.Contains(dexId)) continue;
                var parsed = ParsePool(pool, dexId);
                if (parsed is not null) pools.Add(parsed);
            }
        }

        _logger.LogDebug("New Aerodrome pools fetched: {Count}", pools.Count);
        return pools;
    }

    public async Task<decimal?> GetTokenPriceUsdAsync(string tokenAddress, CancellationToken ct = default)
    {
        var url = _settings.Current.GeckoBaseUrl.TrimEnd('/') +
                  $"/simple/networks/base/token_price/{tokenAddress}";
        var dto = await GetAsync<PriceResponse>(url, ct);
        var prices = dto?.Data?.Attributes?.Prices;
        if (prices is null || prices.Count == 0) return null;
        return Parse(prices.Values.First());
    }

    private static PoolInfo? ParsePool(PoolDto pool, string dexId)
    {
        // TODO: token icons live on the gecko cdn, cache them under %LocalAppData%
        // before wiring icons into the pool rows
        var attr = pool.Attributes;
        if (attr is null) return null;

        var baseId = pool.Relationships?.BaseToken?.Data?.Id;
        var quoteId = pool.Relationships?.QuoteToken?.Data?.Id;
        if (baseId is null || quoteId is null) return null;

        var parts = (attr.Name ?? string.Empty).Split(" / ");
        if (parts.Length < 2) return null;
        var quoteSymbol = parts[1].Split(' ')[0];

        return new PoolInfo
        {
            Address = attr.Address ?? string.Empty,
            DexId = dexId,
            BaseToken = new TokenRef
            {
                Address = StripNetwork(baseId),
                Symbol = parts[0].Trim()
            },
            QuoteToken = new TokenRef
            {
                Address = StripNetwork(quoteId),
                Symbol = quoteSymbol
            },
            PriceUsd = Parse(attr.BaseTokenPriceUsd),
            LiquidityUsd = Parse(attr.ReserveInUsd),
            Volume24hUsd = Parse(attr.VolumeUsd?.H24),
            FdvUsd = Parse(attr.FdvUsd),
            MarketCapUsd = Parse(attr.MarketCapUsd),
            PriceChangePct24h = Parse(attr.PriceChangePercentage?.H24),
            CreatedAt = attr.PoolCreatedAt is null
                ? DateTimeOffset.UtcNow
                : DateTimeOffset.Parse(attr.PoolCreatedAt)
        };
    }

    private static string StripNetwork(string id) =>
        id.Contains('_') ? id[(id.IndexOf('_') + 1)..] : id;

    private async Task<T?> GetAsync<T>(string url, CancellationToken ct) where T : class
    {
        var started = Environment.TickCount64;
        try
        {
            var response = await _http.GetAsync(url, ct);
            _logger.LogDebug("GET {Url} -> {Status} in {Ms}ms", url, (int)response.StatusCode,
                Environment.TickCount64 - started);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("GeckoTerminal {Status}: {Body}", (int)response.StatusCode,
                    body[..Math.Min(500, body.Length)]);
                return null;
            }
            return await response.Content.ReadFromJsonAsync<T>(Json, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "GeckoTerminal request failed: {Url}", url);
            return null;
        }
    }

    private static decimal Parse(string? value) =>
        decimal.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0m;

    private sealed record NewPoolsResponse
    {
        public List<PoolDto>? Data { get; init; }
    }

    private sealed record PoolDto
    {
        public PoolAttributes? Attributes { get; init; }
        public PoolRelationships? Relationships { get; init; }
    }

    private sealed record PoolAttributes
    {
        public string? Name { get; init; }
        [JsonPropertyName("address")] public string? Address { get; init; }
        [JsonPropertyName("base_token_price_usd")] public string? BaseTokenPriceUsd { get; init; }
        [JsonPropertyName("reserve_in_usd")] public string? ReserveInUsd { get; init; }
        [JsonPropertyName("volume_usd")] public VolumeDto? VolumeUsd { get; init; }
        [JsonPropertyName("fdv_usd")] public string? FdvUsd { get; init; }
        [JsonPropertyName("market_cap_usd")] public string? MarketCapUsd { get; init; }
        [JsonPropertyName("price_change_percentage")] public ChangeDto? PriceChangePercentage { get; init; }
        [JsonPropertyName("pool_created_at")] public string? PoolCreatedAt { get; init; }
    }

    private sealed record VolumeDto
    {
        [JsonPropertyName("h24")] public string? H24 { get; init; }
    }

    private sealed record ChangeDto
    {
        [JsonPropertyName("h24")] public string? H24 { get; init; }
    }

    private sealed record PoolRelationships
    {
        [JsonPropertyName("base_token")] public RefWrapper? BaseToken { get; init; }
        [JsonPropertyName("quote_token")] public RefWrapper? QuoteToken { get; init; }
        [JsonPropertyName("dex")] public RefWrapper? Dex { get; init; }
    }

    private sealed record RefWrapper
    {
        public RefData? Data { get; init; }
    }

    private sealed record RefData
    {
        public string? Id { get; init; }
    }

    private sealed record PriceResponse
    {
        public PriceData? Data { get; init; }
    }

    private sealed record PriceData
    {
        public PriceAttributes? Attributes { get; init; }
    }

    private sealed record PriceAttributes
    {
        [JsonPropertyName("token_prices")] public Dictionary<string, string>? Prices { get; init; }
    }
}
