using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using AerodromeSniper.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AerodromeSniper.ViewModels;

public partial class CopyViewModel : PageViewModel
{
    public override string Title => "Copy";
    public override string Subtitle => "Mirror wallets you trust";
    public override Geometry Icon => IconRes.Get("IconCopy");

    public ObservableCollection<WalletRow> Wallets { get; } = [];
    public ObservableCollection<CopyFeedRow> Feed { get; } = [];

    [ObservableProperty]
    private bool _hasWallets = true;

    public CopyViewModel()
    {
        // TODO: per-wallet win rate needs its own small store,
        // computing it from the live feed forgets everything on restart
        var green = (SolidColorBrush)Application.Current.FindResource("ProfitSoftBrush");
        var blue = (SolidColorBrush)Application.Current.FindResource("PrimarySoftBrush");
        var violet = (SolidColorBrush)Application.Current.FindResource("VioletSoftBrush");

        Wallets.Add(new WalletRow
        {
            Address = "0x7a3f…9c21",
            Label = "Smart whale",
            Initials = "SW",
            Accent = blue,
            CopyModeText = "$300 per trade",
            StatsText = "6 trades · 68% win rate · 24h",
            PnlText = "+$2,140.80",
            PnlPct = 12.4
        });
        Wallets.Add(new WalletRow
        {
            Address = "0x2e8b…04aa",
            Label = "Aero degen",
            Initials = "AD",
            Accent = violet,
            CopyModeText = "10% of their size",
            StatsText = "4 trades · 52% win rate · 24h",
            PnlText = "+$380.15",
            PnlPct = 3.1
        });
        Wallets.Add(new WalletRow
        {
            Address = "0x9c31…77d0",
            Label = "LP farmer",
            Initials = "LP",
            Accent = green,
            CopyModeText = "$100 per trade",
            StatsText = "1 trade · 0% win rate · 24h",
            PnlText = "-$42.00",
            PnlPct = -1.8,
            Enabled = false
        });

        var filled = (SolidColorBrush)Application.Current.FindResource("ProfitBrush");
        var filledBg = (SolidColorBrush)Application.Current.FindResource("ProfitSoftBrush");
        var pending = (SolidColorBrush)Application.Current.FindResource("WarningBrush");
        var pendingBg = (SolidColorBrush)Application.Current.FindResource("WarningSoftBrush");

        Feed.Add(new CopyFeedRow { TimeText = "2 min ago", WalletLabel = "Smart whale", TheirText = "Bought TOSHI · $4,200", OurText = "Bought $300", StatusText = "Filled", StatusBrush = filled, StatusBackground = filledBg });
        Feed.Add(new CopyFeedRow { TimeText = "18 min ago", WalletLabel = "Aero degen", TheirText = "Bought CLANKER · $1,800", OurText = "Bought $180", StatusText = "Filled", StatusBrush = filled, StatusBackground = filledBg });
        Feed.Add(new CopyFeedRow { TimeText = "41 min ago", WalletLabel = "Smart whale", TheirText = "Sold AERO · $6,100", OurText = "Sold position", StatusText = "Filled", StatusBrush = filled, StatusBackground = filledBg });
        Feed.Add(new CopyFeedRow { TimeText = "1 h ago", WalletLabel = "LP farmer", TheirText = "Bought MIGGLES · $900", OurText = "—", StatusText = "Skipped", StatusBrush = pending, StatusBackground = pendingBg });
        Feed.Add(new CopyFeedRow { TimeText = "3 h ago", WalletLabel = "Aero degen", TheirText = "Bought VIRTUAL · $2,400", OurText = "Bought $240", StatusText = "Filled", StatusBrush = filled, StatusBackground = filledBg });
    }

    [RelayCommand]
    private void AddWallet(WalletRow wallet)
    {
        Wallets.Add(wallet);
        HasWallets = true;
    }

    [RelayCommand]
    private void RemoveWallet(WalletRow wallet)
    {
        Wallets.Remove(wallet);
        HasWallets = Wallets.Count > 0;
    }
}
