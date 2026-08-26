using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using AerodromeSniper.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AerodromeSniper.ViewModels;

public partial class SniperViewModel : PageViewModel
{
    private readonly List<PoolRow> _allPools = [];
    private int _scanCounter;

    public override string Title => "Sniper";
    public override string Subtitle => "New Aerodrome pools, live";
    public override Geometry Icon => IconRes.Get("IconCrosshair");

    public ObservableCollection<PoolRow> Pools { get; } = [];

    [ObservableProperty]
    private PoolRow? _selectedPool;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _minLiquidityIndex;

    [ObservableProperty]
    private int _maxAgeIndex;

    [ObservableProperty]
    private int _quoteIndex;

    public SniperViewModel()
    {
        SeedPools();
        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnMinLiquidityIndexChanged(int value) => ApplyFilters();
    partial void OnMaxAgeIndexChanged(int value) => ApplyFilters();
    partial void OnQuoteIndexChanged(int value) => ApplyFilters();

    [RelayCommand]
    private void ScanNow()
    {
        var pool = MakePool(_scanCounter++);
        _allPools.Insert(0, pool);
        ApplyFilters();
    }

    [RelayCommand]
    private void SelectPool(PoolRow pool) => SelectedPool = pool;

    private void ApplyFilters()
    {
        var minLiquidity = MinLiquidityIndex switch { 1 => 1_000d, 2 => 10_000d, 3 => 50_000d, _ => 0d };
        var maxAge = MaxAgeIndex switch { 1 => 5d, 2 => 60d, 3 => 1440d, _ => double.MaxValue };
        var quote = QuoteIndex switch { 1 => "WETH", 2 => "USDC", _ => null };

        Pools.Clear();
        foreach (var pool in _allPools)
        {
            if (pool.LiquidityUsd < minLiquidity) continue;
            if (pool.AgeMinutes > maxAge) continue;
            if (quote is not null && pool.QuoteSymbol != quote) continue;
            if (!string.IsNullOrWhiteSpace(SearchText) &&
                !pool.BaseSymbol.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) continue;
            Pools.Add(pool);
        }
    }

    private void SeedPools()
    {
        _allPools.AddRange(
        [
            Make("0x4b9e…2f1a", "KEYCAT", "WETH", "KE", "v2", 2, "2 min", 31_200, "$31,200", "$42,100", "$0.0028", 22.4, isNew: true),
            Make("0x9c31…77d0", "WELL", "WETH", "WE", "slipstream", 4, "4 min", 18_700, "$18,700", "$26,400", "$0.00094", 5.5, isNew: true),
            Make("0x7a3f…9c21", "CLANKER", "WETH", "CL", "v2", 18, "18 min", 120_500, "$120,500", "$410,000", "$18.42", 9.7),
            Make("0x2e8b…04aa", "TOSHI", "WETH", "TO", "slipstream", 34, "34 min", 98_400, "$98,400", "$310,000", "$0.00021", 12.8),
            Make("0x53c2…b8e7", "SPX", "WETH", "SP", "v2", 55, "55 min", 77_900, "$77,900", "$95,300", "$0.41", -3.2),
            Make("0x88d4…1c09", "MIGGLES", "USDC", "MI", "v2", 90, "1 h", 54_100, "$54,100", "$88,700", "$0.0312", -1.4),
            Make("0x6f10…4d33", "DEGEN", "WETH", "DE", "v2", 150, "2 h", 230_700, "$230,700", "$640,000", "$0.0061", 6.1),
            Make("0x1c77…e5b2", "VIRTUAL", "WETH", "VI", "slipstream", 260, "4 h", 640_200, "$640,200", "$2,100,000", "$0.94", 3.3),
            Make("0x3a95…60f8", "BRETT", "WETH", "BR", "v2", 480, "8 h", 412_300, "$412,300", "$1,200,000", "$0.0842", 4.2),
            Make("0xb206…91ce", "AERO", "WETH", "AE", "v2", 1500, "25 h", 1_800_000, "$1.8M", "$4,200,000", "$1.13", 0.8)
        ]);
    }

    private PoolRow MakePool(int index)
    {
        string[] symbols = ["NOVA", "ZORB", "FLUX", "QUANT", "ORBIT"];
        var symbol = symbols[index % symbols.Length] + index / symbols.Length;
        return Make("0x0000…0000", symbol, "WETH", symbol[..2], "v2", 0, "just now",
            12_000 + index * 700, "$" + (12_000 + index * 700).ToString("N0"), "$15,000", "$0.0001", 1.0, isNew: true);
    }

    private static PoolRow Make(string address, string baseSymbol, string quote, string initials, string dex,
        double ageMinutes, string ageText, double liquidity, string liquidityText, string volumeText,
        string priceText, double changePct, bool isNew = false)
    {
        var palette = new[] { "PrimarySoftBrush", "VioletSoftBrush", "ProfitSoftBrush", "WarningSoftBrush" };
        var key = palette[Math.Abs(baseSymbol.GetHashCode()) % palette.Length];
        return new PoolRow
        {
            Address = address,
            BaseSymbol = baseSymbol,
            QuoteSymbol = quote,
            Initials = initials,
            Accent = (SolidColorBrush)Application.Current.FindResource(key),
            DexTag = dex,
            AgeMinutes = ageMinutes,
            AgeText = ageText,
            LiquidityUsd = liquidity,
            LiquidityText = liquidityText,
            VolumeText = volumeText,
            PriceText = priceText,
            ChangePct = changePct,
            IsNew = isNew
        };
    }
}
