using AerodromeSniper.Models;

namespace AerodromeSniper.Services.Chain;

public interface IWalletActivityService
{
    Task<IReadOnlyList<WalletTransfer>> GetTransfersAsync(string walletAddress, CancellationToken ct = default);
}

public interface IChainStatusService
{
    Task<long> GetBlockNumberAsync(CancellationToken ct = default);
}
