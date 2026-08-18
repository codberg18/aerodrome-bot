using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using AerodromeSniper.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AerodromeSniper.ViewModels;

public class DashboardViewModel : PageViewModel
{
    public override string Title => "Dashboard";
    public override string Subtitle => "Account overview at a glance";
    public override Geometry Icon => IconRes.Get("IconGrid");

    public string TotalValueUsd { get; } = "$14,832.40";
    public string TotalValueChange { get; } = "+3.2% today";
    public string TotalPnlUsd { get; } = "+$1,204.18";
    public double TotalPnlPct { get; } = 8.84;
    public string DayPnlUsd { get; } = "+$212.55";
    public double DayPnlPct { get; } = 1.45;
    public string OpenPositionsCount { get; } = "4";
    public string OpenPositionsHint { get; } = "2 snipes · 1 copy · 1 manual";

    public ObservableCollection<PositionRow> Positions { get; } = [];
    public ObservableCollection<ActivityRow> Activity { get; } = [];

    public ISeries[] EquitySeries { get; }
    public Axis[] XAxes { get; }
    public Axis[] YAxes { get; }

    public DashboardViewModel()
    {
        // TODO: persist equity snapshots once a day so the curve survives restarts
        var blue = (SolidColorBrush)Application.Current.FindResource("PrimarySoftBrush");
        var violet = (SolidColorBrush)Application.Current.FindResource("VioletSoftBrush");
        var green = (SolidColorBrush)Application.Current.FindResource("ProfitSoftBrush");
        var amber = (SolidColorBrush)Application.Current.FindResource("WarningSoftBrush");

        Positions.Add(new PositionRow { Symbol = "PEPE", Initials = "PE", QuantityText = "12,400,000 PEPE", ValueUsd = "$1,842.10", PnlPct = 14.2, Accent = green });
        Positions.Add(new PositionRow { Symbol = "AERO", Initials = "AE", QuantityText = "1,250 AERO", ValueUsd = "$1,412.50", PnlPct = -2.4, Accent = blue });
        Positions.Add(new PositionRow { Symbol = "TOSHI", Initials = "TO", QuantityText = "45,000 TOSHI", ValueUsd = "$921.44", PnlPct = 6.8, Accent = violet });
        Positions.Add(new PositionRow { Symbol = "DEGEN", Initials = "DE", QuantityText = "8,200 DEGEN", ValueUsd = "$388.66", PnlPct = 31.5, Accent = amber });

        Activity.Add(new ActivityRow { Kind = "Buy", Text = "Bought 12,400,000 PEPE", TimeText = "2 min ago", KindBrush = (SolidColorBrush)Application.Current.FindResource("ProfitBrush") });
        Activity.Add(new ActivityRow { Kind = "Snipe", Text = "Sniped TOSHI/WETH pool", TimeText = "18 min ago", KindBrush = (SolidColorBrush)Application.Current.FindResource("PrimaryBrush") });
        Activity.Add(new ActivityRow { Kind = "Copy", Text = "Copied 0x7a3f…9c21 buying AERO", TimeText = "41 min ago", KindBrush = (SolidColorBrush)Application.Current.FindResource("AccentVioletBrush") });
        Activity.Add(new ActivityRow { Kind = "Sell", Text = "Sold 8,200 DEGEN", TimeText = "1 h ago", KindBrush = (SolidColorBrush)Application.Current.FindResource("LossBrush") });
        Activity.Add(new ActivityRow { Kind = "Buy", Text = "Bought 1,250 AERO", TimeText = "3 h ago", KindBrush = (SolidColorBrush)Application.Current.FindResource("ProfitBrush") });

        var values = BuildEquityCurve();
        EquitySeries =
        [
            new LineSeries<double>
            {
                Values = values,
                GeometrySize = 0,
                LineSmoothness = 0.5,
                Stroke = new SolidColorPaint(new SKColor(0x3D, 0x74, 0xF7)) { StrokeThickness = 2 },
                Fill = new LinearGradientPaint(
                    [new SKColor(0x26, 0x60, 0xF5, 80), new SKColor(0x26, 0x60, 0xF5, 0)],
                    new SKPoint(0, 0), new SKPoint(0, 1))
            }
        ];
        XAxes = [new Axis { IsVisible = false }];
        YAxes = [new Axis { IsVisible = false, MinLimit = values.Min() * 0.985, MaxLimit = values.Max() * 1.01 }];
    }

    private static double[] BuildEquityCurve()
    {
        var random = new Random(7);
        var points = new double[48];
        var value = 13_600d;
        for (var i = 0; i < points.Length; i++)
        {
            value += random.NextDouble() * 120 - 45;
            points[i] = value;
        }
        return points;
    }
}
