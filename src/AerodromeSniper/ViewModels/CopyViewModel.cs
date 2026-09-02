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
