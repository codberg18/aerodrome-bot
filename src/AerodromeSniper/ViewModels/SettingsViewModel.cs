using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AerodromeSniper.ViewModels;

public partial class SettingsViewModel : PageViewModel
{
    public override string Title => "Settings";
    public override string Subtitle => "Polling, endpoints and account";
    public override Geometry Icon => IconRes.Get("IconSliders");

    [ObservableProperty]
    private string _newPoolsInterval = "15";

    [ObservableProperty]
    private string _pricesInterval = "10";

    [ObservableProperty]
    private string _walletsInterval = "20";

    [ObservableProperty]
    private string _geckoEndpoint = "https://api.geckoterminal.com/api/v2";

    [ObservableProperty]
    private string _blockscoutEndpoint = "https://base.blockscout.com/api/v2";

    [ObservableProperty]
    private string _rpcEndpoint = "https://mainnet.base.org";

    [ObservableProperty]
    private string _slippagePct = "1.0";

    [ObservableProperty]
    private string _feePct = "0.3";

    public string DataFolderPath { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AerodromeSniper");

    [RelayCommand]
    private void ResetAccount()
    {
        var result = MessageBox.Show(
            "This clears balances, positions and trade history on this machine and restores the starting balance. Continue?",
            "Reset account",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            // wired to the account store in a later step
        }
    }

    [RelayCommand]
    private void OpenDataFolder()
    {
        Directory.CreateDirectory(DataFolderPath);
        Process.Start(new ProcessStartInfo { FileName = DataFolderPath, UseShellExecute = true });
    }
}
