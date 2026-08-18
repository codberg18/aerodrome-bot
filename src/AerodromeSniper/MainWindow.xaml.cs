using System.Windows;
using AerodromeSniper.ViewModels;

namespace AerodromeSniper;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
