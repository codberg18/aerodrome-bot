namespace AerodromeSniper.Models;

public class AppSettings
{
    public int NewPoolsIntervalSec { get; set; } = 15;
    public int PricesIntervalSec { get; set; } = 10;
    public int WalletsIntervalSec { get; set; } = 20;
    public string GeckoBaseUrl { get; set; } = "https://api.geckoterminal.com/api/v2";
    public string BlockscoutBaseUrl { get; set; } = "https://base.blockscout.com/api/v2";
    public string RpcUrl { get; set; } = "https://mainnet.base.org";
    public decimal SlippagePct { get; set; } = 1.0m;
    public decimal FeePct { get; set; } = 0.3m;
}
