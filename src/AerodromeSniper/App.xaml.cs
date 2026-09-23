using System.IO;
using System.Windows;
using AerodromeSniper.Services.Chain;
using AerodromeSniper.Services.Market;
using AerodromeSniper.Services.Storage;
using AerodromeSniper.Services.Trading;
using AerodromeSniper.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AerodromeSniper;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(Paths.LogsDir, "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IJsonStore>(_ => new JsonFileStore(Paths.DataDir));
                services.AddSingleton<ISettingsStore, SettingsStore>();
                services.AddSingleton<ITradeJournal, TradeJournal>();
                services.AddSingleton<ITradingEngine, TradingEngine>();
                services.AddHttpClient<IMarketDataService, GeckoTerminalClient>(c => c.Timeout = TimeSpan.FromSeconds(15));
                services.AddHttpClient<IWalletActivityService, BlockscoutClient>(c => c.Timeout = TimeSpan.FromSeconds(15));
                services.AddHttpClient<IChainStatusService, BaseRpcClient>(c => c.Timeout = TimeSpan.FromSeconds(10));

                services.AddSingleton<MainViewModel>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<SniperViewModel>();
                services.AddSingleton<CopyViewModel>();
                services.AddSingleton<PortfolioViewModel>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _host.Start();
        _host.Services.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host.StopAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
        _host.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
