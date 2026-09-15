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
        var url = _settings.Current.GeckoBaseUrl.TrimEnd('/') + "/networks/base/new_pools?page=1";
        var dto = await GetAsync<NewPoolsResponse>(url, ct);
        if (dto is null) return [];

        var tokens = dto.Included?
            .Where(t => t.Id is not null)
            .ToDictionary(t => t.Id!, t => t) ?? new Dictionary<string, TokenDto>();

        var pools = new List<PoolInfo>();
        foreach (var pool in dto.Data ?? [])
        {
            var baseToken = Resolve(tokens, pool.Relationships?.BaseToken?.Data?.Id);
            var quoteToken = Resolve(tokens, pool.Relationships?.QuoteToken?.Data?.Id);
            if (baseToken is null || quoteToken is null) continue;
            if (pool.Attributes?.DexId is null || !pool.Attributes.DexId.StartsWith("aerodrome", StringComparison.OrdinalIgnoreCase)) continue;

            pools.Add(new PoolInfo
            {
                Address = pool.Attributes.PoolAddress ?? pool.Id?.Replace("base/", "") ?? string.Empty,
                DexId = pool.Attributes.DexId,
                BaseToken = baseToken,
                QuoteToken = quoteToken,
                PriceUsd = Parse(pool.Attributes.PriceUsd),
                LiquidityUsd = Parse(pool.Attributes.ReserveInUsd),
                Volume24hUsd = pool.Attributes.VolumeUsd?.H24 is null ? 0m : Parse(pool.Attributes.VolumeUsd.H24),
                FdvUsd = Parse(pool.Attributes.FdvUsd),
                MarketCapUsd = Parse(pool.Attributes.MarketCapUsd),
                PriceChangePct24h = Parse(pool.Attributes.PriceChangePercentage?.H24),
                CreatedAt = pool.Attributes.PoolCreatedAt is null
                    ? DateTimeOffset.UtcNow
                    : DateTimeOffset.Parse(pool.Attributes.PoolCreatedAt)
            });
        }

        _logger.LogDebug("New pools fetched: {Count}", pools.Count);
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
                _logger.LogWarning("GeckoTerminal {Status}: {Body}", (int)response.StatusCode, body[..Math.Min(500, body.Length)]);
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

    private static TokenRef? Resolve(Dictionary<string, TokenDto> tokens, string? id)
    {
        if (id is null || !tokens.TryGetValue(id, out var dto)) return null;
        return new TokenRef
        {
            Address = id.Contains('/') ? id[(id.IndexOf('/') + 1)..] : id,
            Symbol = dto.Attributes?.Symbol ?? "?",
            Name = dto.Attributes?.Name ?? string.Empty,
            Decimals = dto.Attributes?.Decimals ?? 18,
            IconUrl = dto.Attributes?.IconUrl ?? string.Empty
        };
    }

    private static decimal Parse(string? value) =>
        decimal.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0m;

    private sealed record NewPoolsResponse
    {
        public List<PoolDto>? Data { get; init; }
        public List<TokenDto>? Included { get; init; }
    }

    private sealed record PoolDto
    {
        public string? Id { get; init; }
        public PoolAttributes? Attributes { get; init; }
        public PoolRelationships? Relationships { get; init; }
    }

    private sealed record PoolAttributes
    {
        [JsonPropertyName("dex_id")] public string? DexId { get; init; }
        [JsonPropertyName("address")] public string? PoolAddress { get; init; }
        [JsonPropertyName("price_usd")] public string? PriceUsd { get; init; }
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
    }

    private sealed record RefWrapper
    {
        public RefData? Data { get; init; }
    }

    private sealed record RefData
    {
        public string? Id { get; init; }
    }

    private sealed record TokenDto
    {
        public string? Id { get; init; }
        public TokenAttributes? Attributes { get; init; }
    }

    private sealed record TokenAttributes
    {
        public string? Symbol { get; init; }
        public string? Name { get; init; }
        public int? Decimals { get; init; }
        [JsonPropertyName("icon_url")] public string? IconUrl { get; init; }
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
