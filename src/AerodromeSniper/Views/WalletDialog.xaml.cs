using System.Windows;
using System.Windows.Media;
using AerodromeSniper.Models;

namespace AerodromeSniper.Views;

public partial class WalletDialog : Window
{
    public WalletRow? Wallet { get; private set; }

    public WalletDialog()
    {
        InitializeComponent();
        ModeBox.SelectionChanged += (_, _) => AmountLabel.Text =
            ModeBox.SelectedIndex == 1 ? "PERCENT, %" : "AMOUNT (USDC)";
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var address = AddressBox.Text.Trim();
        if (address.Length < 8) return;

        var shortened = address.Length > 10 ? address[..6] + "…" + address[^4..] : address;
        var label = string.IsNullOrWhiteSpace(LabelBox.Text) ? shortened : LabelBox.Text.Trim();
        var amount = AmountBox.Text.Trim();
        var modeText = ModeBox.SelectedIndex == 1 ? $"{amount}% of their size" : $"${amount} per trade";

        var palette = new[] { "PrimarySoftBrush", "VioletSoftBrush", "ProfitSoftBrush", "WarningSoftBrush" };
        Wallet = new WalletRow
        {
            Address = shortened,
            Label = label,
            Initials = new string(label.Take(2).Select(char.ToUpper).ToArray()),
            Accent = (SolidColorBrush)Application.Current.FindResource(palette[Math.Abs(label.GetHashCode()) % palette.Length]),
            CopyModeText = modeText,
            StatsText = "0 trades · — win rate · 24h",
            PnlText = "$0.00",
            PnlPct = 0,
            Enabled = EnabledToggle.IsChecked == true
        };
        DialogResult = true;
    }
}
