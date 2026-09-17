using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AerodromeSniper.Models;
using AerodromeSniper.Services.Storage;
using Microsoft.Extensions.Logging;

namespace AerodromeSniper.Services.Chain;

public class BlockscoutClient : IWalletActivityService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly ISettingsStore _settings;
    private readonly ILogger<BlockscoutClient> _logger;

    public BlockscoutClient(HttpClient http, ISettingsStore settings, ILogger<BlockscoutClient> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
    }

    public async Task<IReadOnlyList<WalletTransfer>> GetTransfersAsync(string walletAddress, CancellationToken ct = default)
    {
        // NOTE: blockscout public rate limits are undocumented, keep polling slow
        var url = _settings.Current.BlockscoutBaseUrl.TrimEnd('/') +
                  $"/addresses/{walletAddress}/token-transfers?type=ERC-20&limit=20";
        var started = Environment.TickCount64;
        try
        {
            var response = await _http.GetAsync(url, ct);
            _logger.LogDebug("GET {Url} -> {Status} in {Ms}ms", url, (int)response.StatusCode,
                Environment.TickCount64 - started);
            if (!response.IsSuccessStatusCode) return [];

            var dto = await response.Content.ReadFromJsonAsync<TransfersResponse>(Json, ct);
            var result = new List<WalletTransfer>();
            foreach (var item in dto?.Items ?? [])
            {
                // TODO: filter by aerodrome router addresses, right now every
                // erc20 transfer of the wallet shows up as activity
                var raw = item.Total?.Value ?? "0";
                var decimals = item.Token?.Decimals ?? 18;
                var amount = decimal.TryParse(raw, out var v) ? v / (decimal)Math.Pow(10, decimals) : 0m;
                DateTimeOffset stamp;
                try
                {
                    stamp = item.Timestamp is null ? DateTimeOffset.UtcNow : DateTimeOffset.Parse(item.Timestamp);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "bad timestamp on transfer {Hash}", item.TxHash);
                    stamp = DateTimeOffset.UtcNow;
                }
                result.Add(new WalletTransfer
                {
                    TxHash = item.TxHash ?? string.Empty,
                    Timestamp = stamp,
                    TokenAddress = item.Token?.AddressHash ?? string.Empty,
                    Symbol = item.Token?.Symbol ?? string.Empty,
                    Amount = amount,
                    From = item.From?.Hash ?? string.Empty,
                    To = item.To?.Hash ?? string.Empty
                });
            }
            return result;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "Blockscout request failed for {Wallet}", walletAddress);
            return [];
        }
    }

    private sealed record TransfersResponse
    {
        public List<TransferDto>? Items { get; init; }
    }

    private sealed record TransferDto
    {
        [JsonPropertyName("tx_hash")] public string? TxHash { get; init; }
        public string? Timestamp { get; init; }
        public TokenDto? Token { get; init; }
        public TotalDto? Total { get; init; }
        public AddressDto? From { get; init; }
        public AddressDto? To { get; init; }
    }

    private sealed record TokenDto
    {
        [JsonPropertyName("address_hash")] public string? AddressHash { get; init; }
        public string? Symbol { get; init; }
        public int? Decimals { get; init; }
    }

    private sealed record TotalDto
    {
        public string? Value { get; init; }
    }

    private sealed record AddressDto
    {
        public string? Hash { get; init; }
    }
}

public class BaseRpcClient : IChainStatusService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly ISettingsStore _settings;
    private readonly ILogger<BaseRpcClient> _logger;

    public BaseRpcClient(HttpClient http, ISettingsStore settings, ILogger<BaseRpcClient> logger)
    {
        _http = http;
        _settings = settings;
        _logger = logger;
    }

    public async Task<long> GetBlockNumberAsync(CancellationToken ct = default)
    {
        try
        {
            var payload = new { jsonrpc = "2.0", id = 1, method = "eth_blockNumber", @params = Array.Empty<object>() };
            var response = await _http.PostAsJsonAsync(_settings.Current.RpcUrl, payload, ct);
            var dto = await response.Content.ReadFromJsonAsync<RpcResponse>(Json, ct);
            if (dto?.Result is null) return 0;
            return Convert.ToInt64(dto.Result, 16);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or FormatException)
        {
            _logger.LogWarning(ex, "Base RPC eth_blockNumber failed");
            return 0;
        }
    }

    private sealed record RpcResponse
    {
        public string? Result { get; init; }
    }
}
