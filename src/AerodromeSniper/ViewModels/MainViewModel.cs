using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AerodromeSniper.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private PageViewModel _currentPage;

    public ObservableCollection<PageViewModel> Pages { get; }

    public string EthPriceUsd { get; } = "$3,412.55";
    public string EthChangePct { get; } = "+2.14%";
    public string BlockHeight { get; } = "#21,543,987";
    public string GasPrice { get; } = "0.004 gwei";

    public MainViewModel(
        DashboardViewModel dashboard,
        SniperViewModel sniper,
        CopyViewModel copy,
        PortfolioViewModel portfolio,
        SettingsViewModel settings)
    {
        Pages = new ObservableCollection<PageViewModel> { dashboard, sniper, copy, portfolio, settings };
        _currentPage = dashboard;
    }
}
