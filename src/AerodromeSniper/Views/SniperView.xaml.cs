using System.Windows;
using System.Windows.Controls;
using AerodromeSniper.ViewModels;

namespace AerodromeSniper.Views;

public partial class SniperView : UserControl
{
    public SniperView()
    {
        InitializeComponent();
    }

    private void OnAddRule(object sender, RoutedEventArgs e)
    {
        var dialog = new SniperRuleDialog { Owner = Window.GetWindow(this) };
        if (dialog.ShowDialog() == true && dialog.Rule is not null && DataContext is SniperViewModel vm)
        {
            vm.AddRuleCommand.Execute(dialog.Rule);
        }
    }
}
