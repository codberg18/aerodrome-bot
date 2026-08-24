using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AerodromeSniper.Converters;

public class PnlBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            if (d > 0) return Application.Current.FindResource("ProfitBrush");
            if (d < 0) return Application.Current.FindResource("LossBrush");
        }
        return Application.Current.FindResource("TextMutedBrush");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
