using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using AerodromeSniper.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AerodromeSniper.ViewModels;

public partial class PortfolioViewModel : PageViewModel
{
    private readonly List<TradeRow> _allTrades = [];

    public override string Title => "Portfolio";
    public override string Subtitle => "Balances, positions and trade history";
    public override Geometry Icon => IconRes.Get("IconPie");

    public string UsdcBalance { get; } = "$8,412.55";
    public string WethBalance { get; } = "2.84 WETH";
    public string WethBalanceUsd { get; } = "$9,684.10";

    public ObservableCollection<PositionRow> Positions { get; } = [];
    public ObservableCollection<TradeRow> History { get; } = [];

    [ObservableProperty]
    private int _sideIndex;

    [ObservableProperty]
    private int _sourceIndex;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public ISeries[] AllocationSeries { get; }
    public ISeries[] PnlSeries { get; }
    public Axis[] PnlXAxes { get; }
    public Axis[] PnlYAxes { get; }

    public PortfolioViewModel()
    {
        var green = (SolidColorBrush)Application.Current.FindResource("ProfitSoftBrush");
        var blue = (SolidColorBrush)Application.Current.FindResource("PrimarySoftBrush");
        var violet = (SolidColorBrush)Application.Current.FindResource("VioletSoftBrush");
        var amber = (SolidColorBrush)Application.Current.FindResource("WarningSoftBrush");

        Positions.Add(new PositionRow { Symbol = "PEPE", Initials = "PE", QuantityText = "12,400,000", ValueUsd = "$1,842.10", PnlPct = 14.2, Accent = green, EntryText = "$0.000148", LastPriceText = "$0.000149" });
        Positions.Add(new PositionRow { Symbol = "AERO", Initials = "AE", QuantityText = "1,250", ValueUsd = "$1,412.50", PnlPct = -2.4, Accent = blue, EntryText = "$1.1580", LastPriceText = "$1.1300" });
        Positions.Add(new PositionRow { Symbol = "TOSHI", Initials = "TO", QuantityText = "45,000", ValueUsd = "$921.44", PnlPct = 6.8, Accent = violet, EntryText = "$0.000192", LastPriceText = "$0.000205" });
        Positions.Add(new PositionRow { Symbol = "DEGEN", Initials = "DE", QuantityText = "8,200", ValueUsd = "$388.66", PnlPct = 31.5, Accent = amber, EntryText = "$0.00464", LastPriceText = "$0.00610" });

        SeedTrades();
        ApplyFilters();

        AllocationSeries =
        [
            new PieSeries<double> { Values = [8412], Fill = new SolidColorPaint(new SKColor(0x26, 0x60, 0xF5)) },
            new PieSeries<double> { Values = [9684], Fill = new SolidColorPaint(new SKColor(0x61, 0x5C, 0xF6)) },
            new PieSeries<double> { Values = [4564], Fill = new SolidColorPaint(new SKColor(0x4A, 0xDE, 0x80)) }
        ];

        var pnlValues = new[] { 96.1, 61.2, -12.4, 148.3, 62.7 };
        PnlSeries =
        [
            new ColumnSeries<double>
            {
                Values = pnlValues,
                Fill = new SolidColorPaint(new SKColor(0x3D, 0x74, 0xF7)),
                MaxBarWidth = 28,
                Rx = 6,
                Ry = 6
            }
        ];
        PnlXAxes = [new Axis { Labels = ["DEGEN", "KEYCAT", "SPX", "PEPE", "TOSHI"], LabelsPaint = new SolidColorPaint(new SKColor(0x95, 0x9C, 0xBF)), TextSize = 11 }];
        PnlYAxes = [new Axis { IsVisible = false }];
    }

    partial void OnSideIndexChanged(int value) => ApplyFilters();
    partial void OnSourceIndexChanged(int value) => ApplyFilters();
    partial void OnSearchTextChanged(string value) => ApplyFilters();

    [RelayCommand]
    private void ClosePosition(PositionRow position)
    {
        Positions.Remove(position);
        var trade = new TradeRow
        {
            TimeText = "just now",
            Side = "Sell",
            SideBrush = (SolidColorBrush)Application.Current.FindResource("LossBrush"),
            Source = "Manual",
            Symbol = position.Symbol,
            QuantityText = position.QuantityText,
            PriceText = position.LastPriceText,
            ValueText = position.ValueUsd,
            RealizedPnlText = position.PnlPct >= 0 ? "+" : "-",
            RealizedPnlPct = position.PnlPct
        };
        _allTrades.Insert(0, trade);
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var side = SideIndex switch { 1 => "Buy", 2 => "Sell", _ => null };
        var source = SourceIndex switch { 1 => "Manual", 2 => "Sniper", 3 => "Copy", _ => null };

        History.Clear();
        foreach (var trade in _allTrades)
        {
            if (side is not null && trade.Side != side) continue;
            if (source is not null && trade.Source != source) continue;
            if (!string.IsNullOrWhiteSpace(SearchText) &&
                !trade.Symbol.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) continue;
            History.Add(trade);
        }
    }

    private void SeedTrades()
    {
        var buy = (SolidColorBrush)Application.Current.FindResource("ProfitBrush");
        var sell = (SolidColorBrush)Application.Current.FindResource("LossBrush");

        _allTrades.AddRange(
        [
            new TradeRow { TimeText = "2 min ago", Side = "Buy", SideBrush = buy, Source = "Manual", Symbol = "PEPE", QuantityText = "12,400,000", PriceText = "$0.000148", ValueText = "$1,835.20" },
            new TradeRow { TimeText = "18 min ago", Side = "Buy", SideBrush = buy, Source = "Sniper", Symbol = "TOSHI", QuantityText = "45,000", PriceText = "$0.000205", ValueText = "$922.50" },
            new TradeRow { TimeText = "41 min ago", Side = "Buy", SideBrush = buy, Source = "Copy", Symbol = "AERO", QuantityText = "1,250", PriceText = "$1.1580", ValueText = "$1,447.50" },
            new TradeRow { TimeText = "1 h ago", Side = "Sell", SideBrush = sell, Source = "Manual", Symbol = "DEGEN", QuantityText = "8,200", PriceText = "$0.0503", ValueText = "$412.46", RealizedPnlText = "+$96.10", RealizedPnlPct = 30.4 },
            new TradeRow { TimeText = "3 h ago", Side = "Buy", SideBrush = buy, Source = "Copy", Symbol = "VIRTUAL", QuantityText = "260", PriceText = "$0.9230", ValueText = "$240.00" },
            new TradeRow { TimeText = "5 h ago", Side = "Sell", SideBrush = sell, Source = "Sniper", Symbol = "KEYCAT", QuantityText = "90,000", PriceText = "$0.0031", ValueText = "$279.00", RealizedPnlText = "+$61.20", RealizedPnlPct = 28.2 },
            new TradeRow { TimeText = "8 h ago", Side = "Buy", SideBrush = buy, Source = "Manual", Symbol = "BRETT", QuantityText = "2,400", PriceText = "$0.0808", ValueText = "$193.92" },
            new TradeRow { TimeText = "26 h ago", Side = "Sell", SideBrush = sell, Source = "Copy", Symbol = "SPX", QuantityText = "180", PriceText = "$0.4240", ValueText = "$76.32", RealizedPnlText = "-$12.40", RealizedPnlPct = -14.0 }
        ]);
    }
}
