using AerodromeSniper.Models;

namespace AerodromeSniper.Services.Market;

public interface IMarketDataService
{
    Task<IReadOnlyList<PoolInfo>> GetNewPoolsAsync(CancellationToken ct = default);
    Task<decimal?> GetTokenPriceUsdAsync(string tokenAddress, CancellationToken ct = default);
}
