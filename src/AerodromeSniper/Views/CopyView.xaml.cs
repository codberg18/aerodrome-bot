using System.Windows;
using System.Windows.Controls;
using AerodromeSniper.ViewModels;

namespace AerodromeSniper.Views;

public partial class CopyView : UserControl
{
    public CopyView()
    {
        InitializeComponent();
    }

    private void OnAddWallet(object sender, RoutedEventArgs e)
    {
        var dialog = new WalletDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Wallet is not null && DataContext is CopyViewModel vm)
        {
            vm.AddWalletCommand.Execute(dialog.Wallet);
        }
    }
}
