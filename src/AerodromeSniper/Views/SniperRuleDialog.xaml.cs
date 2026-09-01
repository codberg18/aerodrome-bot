using System.Windows;
using AerodromeSniper.Models;

namespace AerodromeSniper.Views;

public partial class SniperRuleDialog : Window
{
    public SniperRuleRow? Rule { get; private set; }

    public SniperRuleDialog()
    {
        InitializeComponent();
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var liq = LiquidityBox.SelectedIndex switch
        {
            1 => "liq ≥ $1,000",
            2 => "liq ≥ $10,000",
            3 => "liq ≥ $50,000",
            _ => "liq any"
        };
        var age = AgeBox.SelectedIndex switch
        {
            1 => "age ≤ 5 min",
            2 => "age ≤ 1 h",
            3 => "age ≤ 24 h",
            _ => "age any"
        };
        var quote = QuoteBox.SelectedIndex switch
        {
            1 => "WETH",
            2 => "USDC",
            _ => "any quote"
        };

        var tp = TpBox.Text.Trim();
        var sl = SlBox.Text.Trim();
        var tpSl = tp.Length > 0 || sl.Length > 0
            ? $"TP +{tp}% · SL -{sl}%"
            : string.Empty;

        Rule = new SniperRuleRow
        {
            Name = string.IsNullOrWhiteSpace(NameBox.Text) ? "Untitled rule" : NameBox.Text.Trim(),
            FiltersText = $"{liq} · {age} · {quote}",
            BuyAmountText = $"${AmountBox.Text.Trim()} per snipe",
            TpSlText = tpSl,
            Enabled = EnabledToggle.IsChecked == true
        };
        DialogResult = true;
    }
}
